using System;
using System.Drawing;
using System.Windows.Forms;
using ScriptEngine;

namespace TNScript
{
    /// <summary>
    /// Summary description for WindowsApp.
    /// </summary>
    public class WindowsApp : BaseApp
    {
        private int currentIconIndex;
        private NotifyIcon icon;
        private Icon[] icons;
        private Timer timer;

        protected override void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "CCCScript", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void ExecutionLoop(IAsyncResult result)
        {
            icon = new NotifyIcon();
            timer = new Timer();
            icons = new Icon[4];

            for (int i = 0; i < 4; i++)
            {
                icons[i] = (Icon) GetResourceObject( (i + 1).ToString());
            }

            icon.Icon = icons[currentIconIndex];
            icon.Text = GetResourceString("IconTip");
            icon.Visible = true;
            icon.DoubleClick += OnIconDoubleClick;

            timer.Tick += OnTimerTick;
            timer.Interval = 350;
            timer.Start();

            Application.Run();

            icon.Dispose();
            timer.Dispose();
        }

        protected override void TerminateExecutionLoop()
        {
            Application.Exit();
        }

        private void OnIconDoubleClick(object sender, EventArgs e)
        {
            if (
                MessageBox.Show(String.Format(GetResourceString("CancelExecution"), ""), EntryAssemblyName,
                                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                TerminateExecution();
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            //Change the icon
            currentIconIndex++;

            if (currentIconIndex == 4)
                currentIconIndex = 0;

            icon.Icon = icons[currentIconIndex];
            icon.Visible = true;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            new WindowsApp().Run(args);
        }
    }
}