using AppKit;
using Foundation;
using System;
using System.Text;
using System.Collections.Generic;

namespace MenubarMusic
{
    public class MusicPlayer
    {
        private String name;
        private String [] playlist;

        public MusicPlayer(String name)
        {
            this.name = name;

            String script = string.Format("tell application \"{0}\" to get name of user playlists", this.name);
            NSAppleScript appleScript = new NSAppleScript(script);
            NSDictionary error;
            NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);

            nint B = result.NumberOfItems;
            playlist = new String[B];
            for (int i = 0; i < playlist.Length; i++)
            {
                playlist[i] = result.DescriptorAtIndex(i + 1).StringValue;
            }

        }



    }


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
            NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);

            Console.OutputEncoding = Encoding.UTF8;
            nint B = result.NumberOfItems;
            String[] A = new String[B];
            for (int i = 0; i < A.Length; i++)
            {
                A[i] = result.DescriptorAtIndex(i + 1).StringValue;
                Console.WriteLine(A[i]);
            }

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
