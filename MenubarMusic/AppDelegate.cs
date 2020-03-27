using AppKit;
using Foundation;
using System;
using System.Text;

namespace MenubarMusic
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            // Insert code here to initialize your application
            //iTunes再生
            //NSAppleScript appleScript = new NSAppleScript("tell application \"iTunes\" to playpause");

            //再生中:kPSP 停止中:kPSp
            //NSAppleScript appleScript = new NSAppleScript("tell application \"iTunes\" to get player state");
            //ユーザーライブラリの取得
            NSAppleScript appleScript = new NSAppleScript("tell application \"iTunes\" to get name of user playlists");
            NSDictionary error;
            //aaaaaaaaaaa
            NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);

            //String [] str = result.Description.Split("\"), \'utxt\'(\"");



            Console.OutputEncoding = Encoding.UTF8;
            nint B = result.NumberOfItems;
            String[] A = new String[B];
            for (int i = 0; i < A.Length; i++)
            {
                A[i] = result.DescriptorAtIndex(i + 1).StringValue;
                Console.WriteLine(A[i]);
            }

            // Insert code here to initialize your application
            NSStatusBar statusBar = NSStatusBar.SystemStatusBar;
            NSMenu menu = new NSMenu("Music");

            //var item = statusBar.CreateStatusItem(NSStatusItemLength.Square);
            var item = statusBar.CreateStatusItem(NSStatusItemLength.Variable);
            item.Title = "Music";
            item.HighlightMode = true;
            item.Length = 250;
            //item.View = new ScrollingStatusItemView();
            item.Menu = menu;

            var menuItem = new NSMenuItem();
            menuItem.Title = "Quit";
            //menuItem.Activated;
            menuItem.Action = new ObjCRuntime.Selector("quit");
            menu.AddItem(menuItem);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
