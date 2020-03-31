using AppKit;
using Foundation;
using System;
using System.Text;
using System.Collections.Generic;
using System.Timers;

namespace MenubarMusic
{
    public class PlayPause
    {

    }

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

        public String GetTrackName()
        {
            String script = string.Format("tell application \"{0}\" to get name of current track", this.name);
            NSAppleScript appleScript = new NSAppleScript(script);
            NSDictionary error;
            NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);

            return result.StringValue;
        }

        public void PlayPause()
        {
            String script = string.Format("tell application \"{0}\" to playpause", this.name);
            NSAppleScript appleScript = new NSAppleScript(script);
            NSDictionary error;
            NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);
        }

        public void AddPlaylistToMenu(NSMenu menu)
        {
            for (int i=0;i<this.playlist.Length; i++)
            {
                var playlistmenu = new NSMenuItem();
                playlistmenu.Title = this.playlist[i];
                playlistmenu.Activated += (sender, e) => {
                    NSAppleScript appleScript = new NSAppleScript("tell application \""+this.name+"\" to play playlist \""+playlistmenu.Title+"\"");
                    NSDictionary error;
                    NSAppleEventDescriptor result = appleScript.ExecuteAndReturnError(out error);
                };
                menu.AddItem(playlistmenu);
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
            item.Menu = menu;

            var itunes = new MusicPlayer("iTunes");

            var menuItem2 = new NSMenuItem();
            menuItem2.Title = "PlayPause";
            menuItem2.Activated += (sender, e) =>
            {
                itunes.PlayPause();
            };
            menu.AddItem(menuItem2);

            var menuItem = new NSMenuItem();
            menuItem.Title = "Quit";
            menuItem.Action = new ObjCRuntime.Selector("quite:");
            menu.AddItem(menuItem);

            itunes.AddPlaylistToMenu(menu);

            var sampleTimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(1.0), delegate {
                item.Title = itunes.GetTrackName();
            });
            sampleTimer.Fire();

            /*
            var timer = new System.Timers.Timer(1000);
            Console.WriteLine(item.Title);
            timer.Elapsed += (sender, e) =>
            {
                Console.WriteLine(itunes.GetTrackName());
            };
            timer.Start();
            */
        }

        [Export("quite:")]
        public void Quite(NSObject sender)
        {
            NSApplication.SharedApplication.Terminate(this);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
