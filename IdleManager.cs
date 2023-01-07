using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace IdleManager
{
    public delegate void IdleCB();

    public class IdleManagerOptions
    {
        public IdleCB OnIdle { get; set; }
        public int IdleTimeout { get; set; } = 10 * 60 * 1000;
        public bool CaptureScroll { get; set; } = false;
        public int ScrollDebounce { get; set; } = 100;
    }

    public class IdleManager
    {
        private List<IdleCB> callbacks = new List<IdleCB>();
        private int idleTimeout;
        private Timer timer;
        private static readonly string[] events = { "mousedown", "mousemove", "keydown", "touchstart", "wheel" };

        public static IdleManager Create(IdleManagerOptions options = null)
        {
            return new IdleManager(options);
        }

        protected IdleManager(IdleManagerOptions options = null)
        {
            if (options == null)
            {
                options = new IdleManagerOptions();
            }

            this.callbacks = options.OnIdle != null ? new List<IdleCB> { options.OnIdle } : new List<IdleCB>();
            this.idleTimeout = options.IdleTimeout;

            timer = new Timer
            {
                Interval = options.IdleTimeout,
                Enabled = true,
                AutoReset = false
            };
            timer.Elapsed += (sender, args) =>
            {
                callbacks.ForEach(cb => cb());
            };

            ResetTimer();

            events.ToList().ForEach(name =>
            {
                System.Windows.Forms.Control.MouseMove += ResetTimer;
                System.Windows.Forms.Control.KeyDown += ResetTimer;
                System.Windows.Forms.Control.MouseDown += ResetTimer;
                System.Windows.Forms.Control.Touch += ResetTimer;
                System.Windows.Forms.Control.MouseWheel += ResetTimer;
            });

            if (options.CaptureScroll)
            {
                System.Windows.Forms.Control.MouseMove += (sender, args) =>
                {
                    timer.Interval = options.ScrollDebounce;
                    timer.Start();
                };
            }
        }

        public void RegisterCallback(IdleCB callback)
        {
            callbacks.Add(callback);
        }

        public void Cleanup()
        {
            timer.Stop();
            timer.Dispose();
            System.Windows.Forms.Control.MouseMove -= ResetTimer;
            System.Windows.Forms.Control.KeyDown -= ResetTimer;
            System.Windows.Forms.Control.MouseDown -= ResetTimer;
            System.Windows.Forms.Control.Touch -= ResetTimer;
            System.Windows.Forms.Control.MouseWheel -= ResetTimer;
        }

        private void ResetTimer(object sender = null, EventArgs e = null)
        {
            timer.Stop();
            timer.Start();
        }
    }
