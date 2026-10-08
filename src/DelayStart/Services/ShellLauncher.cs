using System.Reflection;
using System.Runtime.InteropServices;
using DelayStart.Localization;

namespace DelayStart.Services;

/// <summary>
/// Пуска програма като обикновен потребител, докато самите ние работим като админ.
/// Молим работния плот на Explorer (който е без админ права) да изпълни IShellDispatch2.ShellExecute вместо нас,
/// така че новият процес наследява неговите права. Техниката е от блога на Raymond Chen „The Old New Thing“.
/// </summary>
internal static class ShellLauncher
{
    private static readonly Guid ClsidShellWindows = new("9BA05972-F6A8-11CF-A442-00A0C90A8F39");
    private static readonly Guid SidTopLevelBrowser = new("4C96BE40-915C-11CF-99D3-00AA004AE837");
    private static readonly Guid IidDispatch = new("00020400-0000-0000-C000-000000000046");

    private const int CsidlDesktop = 0;
    private const int SwcDesktop = 8;
    private const int SwfoNeedDispatch = 1;
    private const uint SvgioBackground = 0;
    private const int SwShowNormal = 1;

    public static bool IsShellReady => GetShellWindow() != IntPtr.Zero;

    public static void ShellExecute(string file, string arguments, string workingDirectory)
    {
        var comObjects = new List<object>();
        T Track<T>(T obj) where T : class
        {
            comObjects.Add(obj);
            return obj;
        }

        try
        {
            var shellWindowsType = Type.GetTypeFromCLSID(ClsidShellWindows, throwOnError: true)!;
            var shellWindows = Track((IShellWindows)Activator.CreateInstance(shellWindowsType)!);

            object location = CsidlDesktop;
            object? root = null;
            var desktop = shellWindows.FindWindowSW(ref location, ref root, SwcDesktop, out _, SwfoNeedDispatch)
                ?? throw new InvalidOperationException(Loc.S.ExplorerDesktopNotFound);
            Track(desktop);

            Guid service = SidTopLevelBrowser;
            Guid iidShellBrowser = typeof(IShellBrowser).GUID;
            var browser = Track((IShellBrowser)((IComServiceProvider)desktop).QueryService(ref service, ref iidShellBrowser));

            var view = Track(browser.QueryActiveShellView());

            Guid iidDispatch = IidDispatch;
            var folderView = Track(view.GetItemObject(SvgioBackground, ref iidDispatch));

            // folderView е IShellFolderViewDual, а Application е IShellDispatch2 – и двете се викат през IDispatch.
            var shell = Track(folderView.GetType().InvokeMember("Application", BindingFlags.GetProperty, null, folderView, null)!);
            shell.GetType().InvokeMember("ShellExecute", BindingFlags.InvokeMethod, null, shell,
                [file, arguments, workingDirectory, "", SwShowNormal]);
        }
        finally
        {
            foreach (var obj in comObjects)
            {
                if (Marshal.IsComObject(obj))
                    Marshal.ReleaseComObject(obj);
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    // Интерфейсите по-долу трябва да описват методите точно в реда от Windows SDK (shldisp.h, shobjidl.h),
    // защото се викат по позиция във vtable-а. Ползваме само последния метод на всеки от тях.

    [ComImport, Guid("85CB6900-4D95-11CF-960C-0080C7F4EE85"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    private interface IShellWindows
    {
        int Count { get; }
        [return: MarshalAs(UnmanagedType.IDispatch)] object Item(object index);
        [return: MarshalAs(UnmanagedType.IUnknown)] object _NewEnum();
        int Register([MarshalAs(UnmanagedType.IDispatch)] object pid, int hwnd, int swClass);
        int RegisterPending(int threadId, ref object location, ref object locationRoot, int swClass);
        void Revoke(int cookie);
        void OnNavigate(int cookie, ref object location);
        void OnActivated(int cookie, [MarshalAs(UnmanagedType.VariantBool)] bool active);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        object? FindWindowSW(ref object location, ref object? locationRoot, int swClass, out int hwnd, int options);
    }

    [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IComServiceProvider
    {
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object QueryService(ref Guid service, ref Guid riid);
    }

    [ComImport, Guid("000214E2-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellBrowser
    {
        void GetWindow(out IntPtr hwnd);
        void ContextSensitiveHelp(int enterMode);
        void InsertMenusSB(IntPtr menuShared, IntPtr menuWidths);
        void SetMenuSB(IntPtr menuShared, IntPtr oleMenuRes, IntPtr activeObject);
        void RemoveMenusSB(IntPtr menuShared);
        void SetStatusTextSB(IntPtr statusText);
        void EnableModelessSB(int enable);
        void TranslateAcceleratorSB(IntPtr msg, ushort id);
        void BrowseObject(IntPtr pidl, uint flags);
        void GetViewStateStream(uint mode, out IntPtr stream);
        void GetControlWindow(uint id, out IntPtr hwnd);
        void SendControlMsg(uint id, uint msg, IntPtr wParam, IntPtr lParam, out IntPtr result);
        IShellView QueryActiveShellView();
    }

    [ComImport, Guid("000214E3-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellView
    {
        void GetWindow(out IntPtr hwnd);
        void ContextSensitiveHelp(int enterMode);
        void TranslateAccelerator(IntPtr msg);
        void EnableModeless(int enable);
        void UIActivate(uint state);
        void Refresh();
        void CreateViewWindow(IntPtr previous, IntPtr folderSettings, IntPtr browser, IntPtr rect, out IntPtr hwnd);
        void DestroyViewWindow();
        void GetCurrentInfo(IntPtr folderSettings);
        void AddPropertySheetPages(uint reserved, IntPtr addPage, IntPtr lParam);
        void SaveViewState();
        void SelectItem(IntPtr pidl, uint flags);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        object GetItemObject(uint item, ref Guid riid);
    }
}
