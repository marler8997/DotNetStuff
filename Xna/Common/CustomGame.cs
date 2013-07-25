using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Marler.Xna.Common
{
    /*
    public class Game : IDisposable
    {
        // Fields
        private TimeSpan accumulatedElapsedGameTime;
        private EventHandler<EventArgs> Activated;
        //private GameClock clock;
        private ContentManager content;
        private List<IDrawable> currentlyDrawingComponents = new List<IDrawable>();
        private List<IUpdateable> currentlyUpdatingComponents = new List<IUpdateable>();
        private EventHandler<EventArgs> Deactivated;
        private EventHandler<EventArgs> Disposed;
        private bool doneFirstDraw;
        private bool doneFirstUpdate;
        private List<IDrawable> drawableComponents = new List<IDrawable>();
        private bool drawRunningSlowly;
        private bool endRunRequired;
        private EventHandler<EventArgs> Exiting;
        private bool exitRequested;
        private bool forceElapsedTimeToZero;
        private GameComponentCollection gameComponents;
        private GameServiceContainer gameServices = new GameServiceContainer();
        private GameTime gameTime = new GameTime();
        private IGraphicsDeviceManager graphicsDeviceManager;
        private IGraphicsDeviceService graphicsDeviceService;
        //private GameHost host;
        private TimeSpan inactiveSleepTime;
        private bool inRun;
        private bool isActive;
        private bool isFixedTimeStep = true;
        private bool isMouseVisible;
        private TimeSpan lastFrameElapsedGameTime;
        private LaunchParameters launchParameters;
        private readonly TimeSpan maximumElapsedTime = TimeSpan.FromMilliseconds(500.0);
        private List<IGameComponent> notYetInitialized = new List<IGameComponent>();
        private bool suppressDraw;
        private TimeSpan targetElapsedTime;
        private TimeSpan totalGameTime;
        private List<IUpdateable> updateableComponents = new List<IUpdateable>();
        private int updatesSinceRunningSlowly1 = 0x7fffffff;
        private int updatesSinceRunningSlowly2 = 0x7fffffff;

        // Events
        /*
        public event EventHandler<EventArgs> Activated
        {
            add
            {
                EventHandler<EventArgs> handler2;
                EventHandler<EventArgs> activated = this.Activated;
                do
                {
                    handler2 = activated;
                    EventHandler<EventArgs> handler3 = (EventHandler<EventArgs>)Delegate.Combine(handler2, value);
                    activated = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref this.Activated, handler3, handler2);
                }
                while (activated != handler2);
            }
            remove
            {
                EventHandler<EventArgs> handler2;
                EventHandler<EventArgs> activated = this.Activated;
                do
                {
                    handler2 = activated;
                    EventHandler<EventArgs> handler3 = (EventHandler<EventArgs>)Delegate.Remove(handler2, value);
                    activated = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref this.Activated, handler3, handler2);
                }
                while (activated != handler2);
            }
        }

        public event EventHandler<EventArgs> Deactivated
        {
            add
            {
                EventHandler<EventArgs> handler2;
                EventHandler<EventArgs> deactivated = this.Deactivated;
                do
                {
                    handler2 = deactivated;
                    EventHandler<EventArgs> handler3 = (EventHandler<EventArgs>)Delegate.Combine(handler2, value);
                    deactivated = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref this.Deactivated, handler3, handler2);
                }
                while (deactivated != handler2);
            }
            remove
            {
                EventHandler<EventArgs> handler2;
                EventHandler<EventArgs> deactivated = this.Deactivated;
                do
                {
                    handler2 = deactivated;
                    EventHandler<EventArgs> handler3 = (EventHandler<EventArgs>)Delegate.Remove(handler2, value);
                    deactivated = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref this.Deactivated, handler3, handler2);
                }
                while (deactivated != handler2);
            }
        }

        public event EventHandler<EventArgs> Disposed
        {
            add
            {
                EventHandler<EventArgs> handler2;
                EventHandler<EventArgs> disposed = this.Disposed;
                do
                {
                    handler2 = disposed;
                    EventHandler<EventArgs> handler3 = (EventHandler<EventArgs>)Delegate.Combine(handler2, value);
                    disposed = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref this.Disposed, handler3, handler2);
                }
                while (disposed != handler2);
            }
            remove
            {
                EventHandler<EventArgs> handler2;
                EventHandler<EventArgs> disposed = this.Disposed;
                do
                {
                    handler2 = disposed;
                    EventHandler<EventArgs> handler3 = (EventHandler<EventArgs>)Delegate.Remove(handler2, value);
                    disposed = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref this.Disposed, handler3, handler2);
                }
                while (disposed != handler2);
            }
        }

        public event EventHandler<EventArgs> Exiting
        {
            add
            {
                EventHandler<EventArgs> handler2;
                EventHandler<EventArgs> exiting = this.Exiting;
                do
                {
                    handler2 = exiting;
                    EventHandler<EventArgs> handler3 = (EventHandler<EventArgs>)Delegate.Combine(handler2, value);
                    exiting = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref this.Exiting, handler3, handler2);
                }
                while (exiting != handler2);
            }
            remove
            {
                EventHandler<EventArgs> handler2;
                EventHandler<EventArgs> exiting = this.Exiting;
                do
                {
                    handler2 = exiting;
                    EventHandler<EventArgs> handler3 = (EventHandler<EventArgs>)Delegate.Remove(handler2, value);
                    exiting = Interlocked.CompareExchange<EventHandler<EventArgs>>(ref this.Exiting, handler3, handler2);
                }
                while (exiting != handler2);
            }
        }

        // Methods
        public Game()
        {
            FrameworkDispatcher.Update();
            this.EnsureHost();
            this.launchParameters = new LaunchParameters();
            this.gameComponents = new GameComponentCollection();
            this.gameComponents.ComponentAdded += new EventHandler<GameComponentCollectionEventArgs>(this.GameComponentAdded);
            this.gameComponents.ComponentRemoved += new EventHandler<GameComponentCollectionEventArgs>(this.GameComponentRemoved);
            this.content = new ContentManager(this.gameServices);
            this.host.Window.Paint += new EventHandler<EventArgs>(this.Paint);
            //this.clock = new GameClock();
            this.totalGameTime = TimeSpan.Zero;
            this.accumulatedElapsedGameTime = TimeSpan.Zero;
            this.lastFrameElapsedGameTime = TimeSpan.Zero;
            this.targetElapsedTime = TimeSpan.FromTicks(0x28b0bL);
            this.inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);
        }

        protected virtual bool BeginDraw()
        {
            if ((this.graphicsDeviceManager != null) && !this.graphicsDeviceManager.BeginDraw())
            {
                return false;
            }
            return true;
        }

        protected virtual void BeginRun()
        {
        }

        private void DeviceCreated(object sender, EventArgs e)
        {
            this.LoadContent();
        }

        private void DeviceDisposing(object sender, EventArgs e)
        {
            this.content.Unload();
            this.UnloadContent();
        }

        private void DeviceReset(object sender, EventArgs e)
        {
        }

        private void DeviceResetting(object sender, EventArgs e)
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    IGameComponent[] array = new IGameComponent[this.gameComponents.Count];
                    this.gameComponents.CopyTo(array, 0);
                    for (int i = 0; i < array.Length; i++)
                    {
                        IDisposable disposable = array[i] as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    IDisposable graphicsDeviceManager = this.graphicsDeviceManager as IDisposable;
                    if (graphicsDeviceManager != null)
                    {
                        graphicsDeviceManager.Dispose();
                    }
                    this.UnhookDeviceEvents();
                    if (this.Disposed != null)
                    {
                        this.Disposed(this, EventArgs.Empty);
                    }
                }
            }
        }

        protected virtual void Draw(GameTime gameTime)
        {
            for (int i = 0; i < this.drawableComponents.Count; i++)
            {
                this.currentlyDrawingComponents.Add(this.drawableComponents[i]);
            }
            for (int j = 0; j < this.currentlyDrawingComponents.Count; j++)
            {
                IDrawable drawable = this.currentlyDrawingComponents[j];
                if (drawable.Visible)
                {
                    drawable.Draw(gameTime);
                }
            }
            this.currentlyDrawingComponents.Clear();
        }

        private void DrawableDrawOrderChanged(object sender, EventArgs e)
        {
            IDrawable item = sender as IDrawable;
            this.drawableComponents.Remove(item);
            int index = this.drawableComponents.BinarySearch(item, DrawOrderComparer.Default);
            if (index < 0)
            {
                index = ~index;
                while ((index < this.drawableComponents.Count) && (this.drawableComponents[index].DrawOrder == item.DrawOrder))
                {
                    index++;
                }
                this.drawableComponents.Insert(index, item);
            }
        }

        private void DrawFrame()
        {
            try
            {
                if (((!this.ShouldExit && this.doneFirstUpdate) && !this.Window.IsMinimized) && this.BeginDraw())
                {
                    this.gameTime.TotalGameTime = this.totalGameTime;
                    this.gameTime.ElapsedGameTime = this.lastFrameElapsedGameTime;
                    this.gameTime.IsRunningSlowly = this.drawRunningSlowly;
                    this.Draw(this.gameTime);
                    this.EndDraw();
                    this.doneFirstDraw = true;
                }
            }
            finally
            {
                this.lastFrameElapsedGameTime = TimeSpan.Zero;
            }
        }

        protected virtual void EndDraw()
        {
            if (this.graphicsDeviceManager != null)
            {
                this.graphicsDeviceManager.EndDraw();
            }
        }

        protected virtual void EndRun()
        {
        }

        private void EnsureHost()
        {
            if (this.host == null)
            {
                this.host = new WindowsGameHost(this);
                this.host.Activated += new EventHandler<EventArgs>(this.HostActivated);
                this.host.Deactivated += new EventHandler<EventArgs>(this.HostDeactivated);
                this.host.Suspend += new EventHandler<EventArgs>(this.HostSuspend);
                this.host.Resume += new EventHandler<EventArgs>(this.HostResume);
                this.host.Idle += new EventHandler<EventArgs>(this.HostIdle);
                this.host.Exiting += new EventHandler<EventArgs>(this.HostExiting);
            }
        }

        public void Exit()
        {
            this.exitRequested = true;
            this.host.Exit();
            if (this.inRun && this.endRunRequired)
            {
                this.EndRun();
                this.inRun = false;
            }
        }

        ~Game()
        {
            this.Dispose(false);
        }

        private void GameComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            if (this.inRun)
            {
                e.GameComponent.Initialize();
            }
            else
            {
                this.notYetInitialized.Add(e.GameComponent);
            }
            IUpdateable gameComponent = e.GameComponent as IUpdateable;
            if (gameComponent != null)
            {
                int index = this.updateableComponents.BinarySearch(gameComponent, UpdateOrderComparer.Default);
                if (index < 0)
                {
                    index = ~index;
                    while ((index < this.updateableComponents.Count) && (this.updateableComponents[index].UpdateOrder == gameComponent.UpdateOrder))
                    {
                        index++;
                    }
                    this.updateableComponents.Insert(index, gameComponent);
                    gameComponent.UpdateOrderChanged += new EventHandler<EventArgs>(this.UpdateableUpdateOrderChanged);
                }
            }
            IDrawable item = e.GameComponent as IDrawable;
            if (item != null)
            {
                int num2 = this.drawableComponents.BinarySearch(item, DrawOrderComparer.Default);
                if (num2 < 0)
                {
                    num2 = ~num2;
                    while ((num2 < this.drawableComponents.Count) && (this.drawableComponents[num2].DrawOrder == item.DrawOrder))
                    {
                        num2++;
                    }
                    this.drawableComponents.Insert(num2, item);
                    item.DrawOrderChanged += new EventHandler<EventArgs>(this.DrawableDrawOrderChanged);
                }
            }
        }

        private void GameComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            if (!this.inRun)
            {
                this.notYetInitialized.Remove(e.GameComponent);
            }
            IUpdateable gameComponent = e.GameComponent as IUpdateable;
            if (gameComponent != null)
            {
                this.updateableComponents.Remove(gameComponent);
                gameComponent.UpdateOrderChanged -= new EventHandler<EventArgs>(this.UpdateableUpdateOrderChanged);
            }
            IDrawable item = e.GameComponent as IDrawable;
            if (item != null)
            {
                this.drawableComponents.Remove(item);
                item.DrawOrderChanged -= new EventHandler<EventArgs>(this.DrawableDrawOrderChanged);
            }
        }

        private void HookDeviceEvents()
        {
            this.graphicsDeviceService = this.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
            if (this.graphicsDeviceService != null)
            {
                this.graphicsDeviceService.DeviceCreated += new EventHandler<EventArgs>(this.DeviceCreated);
                this.graphicsDeviceService.DeviceResetting += new EventHandler<EventArgs>(this.DeviceResetting);
                this.graphicsDeviceService.DeviceReset += new EventHandler<EventArgs>(this.DeviceReset);
                this.graphicsDeviceService.DeviceDisposing += new EventHandler<EventArgs>(this.DeviceDisposing);
            }
        }

        private void HostActivated(object sender, EventArgs e)
        {
            if (!this.isActive)
            {
                this.isActive = true;
                this.OnActivated(this, EventArgs.Empty);
            }
        }

        private void HostDeactivated(object sender, EventArgs e)
        {
            if (this.isActive)
            {
                this.isActive = false;
                this.OnDeactivated(this, EventArgs.Empty);
            }
        }

        private void HostExiting(object sender, EventArgs e)
        {
            this.OnExiting(this, EventArgs.Empty);
        }

        private void HostIdle(object sender, EventArgs e)
        {
            this.Tick();
        }

        private void HostResume(object sender, EventArgs e)
        {
            this.clock.Resume();
        }

        private void HostSuspend(object sender, EventArgs e)
        {
            this.clock.Suspend();
        }
        protected virtual void Initialize()
        {
            this.HookDeviceEvents();
            while (this.notYetInitialized.Count != 0)
            {
                this.notYetInitialized[0].Initialize();
                this.notYetInitialized.RemoveAt(0);
            }
            if ((this.graphicsDeviceService != null) && (this.graphicsDeviceService.GraphicsDevice != null))
            {
                this.LoadContent();
            }
        }

        protected virtual void LoadContent()
        {
        }

        protected virtual void OnActivated(object sender, EventArgs args)
        {
            if (this.Activated != null)
            {
                this.Activated(this, args);
            }
        }

        protected virtual void OnDeactivated(object sender, EventArgs args)
        {
            if (this.Deactivated != null)
            {
                this.Deactivated(this, args);
            }
        }

        protected virtual void OnExiting(object sender, EventArgs args)
        {
            if (this.Exiting != null)
            {
                this.Exiting(null, args);
            }
        }

        private void Paint(object sender, EventArgs e)
        {
            if (this.doneFirstDraw)
            {
                this.DrawFrame();
            }
        }

        public void ResetElapsedTime()
        {
            this.forceElapsedTimeToZero = true;
            this.drawRunningSlowly = false;
            this.updatesSinceRunningSlowly1 = 0x7fffffff;
            this.updatesSinceRunningSlowly2 = 0x7fffffff;
        }

        public void Run()
        {
            this.RunGame(true);
        }

        private void RunGame(bool useBlockingRun)
        {
            try
            {
                this.graphicsDeviceManager = this.Services.GetService(typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;
                if (this.graphicsDeviceManager != null)
                {
                    this.graphicsDeviceManager.CreateDevice();
                }
                this.Initialize();
                this.inRun = true;
                this.BeginRun();
                this.gameTime.ElapsedGameTime = TimeSpan.Zero;
                this.gameTime.TotalGameTime = this.totalGameTime;
                this.gameTime.IsRunningSlowly = false;
                this.Update(this.gameTime);
                this.doneFirstUpdate = true;
                if (useBlockingRun)
                {
                    if (this.host != null)
                    {
                        this.host.Run();
                    }
                    this.EndRun();
                }
                else
                {
                    if (this.host != null)
                    {
                        this.host.StartGameLoop();
                    }
                    this.endRunRequired = true;
                }
            }
            catch (NoSuitableGraphicsDeviceException exception)
            {
                if (!this.ShowMissingRequirementMessage(exception))
                {
                    throw;
                }
            }
            catch (NoAudioHardwareException exception2)
            {
                if (!this.ShowMissingRequirementMessage(exception2))
                {
                    throw;
                }
            }
            finally
            {
                if (!this.endRunRequired)
                {
                    this.inRun = false;
                }
            }
        }

        public void RunOneFrame()
        {
            if (this.host != null)
            {
                this.host.RunOneFrame();
            }
        }

        protected virtual bool ShowMissingRequirementMessage(Exception exception)
        {
            return ((this.host != null) && this.host.ShowMissingRequirementMessage(exception));
        }

        internal void StartGameLoop()
        {
            this.RunGame(false);
        }

        public void SuppressDraw()
        {
            this.suppressDraw = true;
        }

        public void Tick()
        {
            if (!this.ShouldExit)
            {
                if (!this.isActive)
                {
                    Thread.Sleep((int)this.inactiveSleepTime.TotalMilliseconds);
                }
                this.clock.Step();
                bool flag = true;
                TimeSpan elapsedAdjustedTime = this.clock.ElapsedAdjustedTime;
                if (elapsedAdjustedTime < TimeSpan.Zero)
                {
                    elapsedAdjustedTime = TimeSpan.Zero;
                }
                if (this.forceElapsedTimeToZero)
                {
                    elapsedAdjustedTime = TimeSpan.Zero;
                    this.forceElapsedTimeToZero = false;
                }
                if (elapsedAdjustedTime > this.maximumElapsedTime)
                {
                    elapsedAdjustedTime = this.maximumElapsedTime;
                }
                if (this.isFixedTimeStep)
                {
                    if (Math.Abs((long)(elapsedAdjustedTime.Ticks - this.targetElapsedTime.Ticks)) < (this.targetElapsedTime.Ticks >> 6))
                    {
                        elapsedAdjustedTime = this.targetElapsedTime;
                    }
                    this.accumulatedElapsedGameTime += elapsedAdjustedTime;
                    long num = this.accumulatedElapsedGameTime.Ticks / this.targetElapsedTime.Ticks;
                    this.accumulatedElapsedGameTime = TimeSpan.FromTicks(this.accumulatedElapsedGameTime.Ticks % this.targetElapsedTime.Ticks);
                    this.lastFrameElapsedGameTime = TimeSpan.Zero;
                    if (num == 0L)
                    {
                        return;
                    }
                    TimeSpan targetElapsedTime = this.targetElapsedTime;
                    if (num > 1L)
                    {
                        this.updatesSinceRunningSlowly2 = this.updatesSinceRunningSlowly1;
                        this.updatesSinceRunningSlowly1 = 0;
                    }
                    else
                    {
                        if (this.updatesSinceRunningSlowly1 < 0x7fffffff)
                        {
                            this.updatesSinceRunningSlowly1++;
                        }
                        if (this.updatesSinceRunningSlowly2 < 0x7fffffff)
                        {
                            this.updatesSinceRunningSlowly2++;
                        }
                    }
                    this.drawRunningSlowly = this.updatesSinceRunningSlowly2 < 20;
                    while ((num > 0L) && !this.ShouldExit)
                    {
                        num -= 1L;
                        try
                        {
                            this.gameTime.ElapsedGameTime = targetElapsedTime;
                            this.gameTime.TotalGameTime = this.totalGameTime;
                            this.gameTime.IsRunningSlowly = this.drawRunningSlowly;
                            this.Update(this.gameTime);
                            flag &= this.suppressDraw;
                            this.suppressDraw = false;
                        }
                        finally
                        {
                            this.lastFrameElapsedGameTime += targetElapsedTime;
                            this.totalGameTime += targetElapsedTime;
                        }
                    }
                }
                else
                {
                    TimeSpan span3 = elapsedAdjustedTime;
                    this.drawRunningSlowly = false;
                    this.updatesSinceRunningSlowly1 = 0x7fffffff;
                    this.updatesSinceRunningSlowly2 = 0x7fffffff;
                    if (!this.ShouldExit)
                    {
                        try
                        {
                            this.gameTime.ElapsedGameTime = this.lastFrameElapsedGameTime = span3;
                            this.gameTime.TotalGameTime = this.totalGameTime;
                            this.gameTime.IsRunningSlowly = false;
                            this.Update(this.gameTime);
                            flag &= this.suppressDraw;
                            this.suppressDraw = false;
                        }
                        finally
                        {
                            this.totalGameTime += span3;
                        }
                    }
                }
                if (!flag)
                {
                    this.DrawFrame();
                }
            }
        }

        private void UnhookDeviceEvents()
        {
            if (this.graphicsDeviceService != null)
            {
                this.graphicsDeviceService.DeviceCreated -= new EventHandler<EventArgs>(this.DeviceCreated);
                this.graphicsDeviceService.DeviceResetting -= new EventHandler<EventArgs>(this.DeviceResetting);
                this.graphicsDeviceService.DeviceReset -= new EventHandler<EventArgs>(this.DeviceReset);
                this.graphicsDeviceService.DeviceDisposing -= new EventHandler<EventArgs>(this.DeviceDisposing);
            }
        }

        protected virtual void UnloadContent()
        {
        }

        protected virtual void Update(GameTime gameTime)
        {
            for (int i = 0; i < this.updateableComponents.Count; i++)
            {
                this.currentlyUpdatingComponents.Add(this.updateableComponents[i]);
            }
            for (int j = 0; j < this.currentlyUpdatingComponents.Count; j++)
            {
                IUpdateable updateable = this.currentlyUpdatingComponents[j];
                if (updateable.Enabled)
                {
                    updateable.Update(gameTime);
                }
            }
            this.currentlyUpdatingComponents.Clear();
            FrameworkDispatcher.Update();
            this.doneFirstUpdate = true;
        }

        private void UpdateableUpdateOrderChanged(object sender, EventArgs e)
        {
            IUpdateable item = sender as IUpdateable;
            this.updateableComponents.Remove(item);
            int index = this.updateableComponents.BinarySearch(item, UpdateOrderComparer.Default);
            if (index < 0)
            {
                index = ~index;
                while ((index < this.updateableComponents.Count) && (this.updateableComponents[index].UpdateOrder == item.UpdateOrder))
                {
                    index++;
                }
                this.updateableComponents.Insert(index, item);
            }
        }

        // Properties
        public GameComponentCollection Components
        {
            get
            {
                return this.gameComponents;
            }
        }

        public ContentManager Content
        {
            get
            {
                return this.content;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                this.content = value;
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                IGraphicsDeviceService graphicsDeviceService = this.graphicsDeviceService;
                if (graphicsDeviceService == null)
                {
                    graphicsDeviceService = this.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
                    if (graphicsDeviceService == null)
                    {
                        throw new InvalidOperationException(Resources.NoGraphicsDeviceService);
                    }
                }
                return graphicsDeviceService.GraphicsDevice;
            }
        }

        public TimeSpan InactiveSleepTime
        {
            get
            {
                return this.inactiveSleepTime;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("value", Resources.InactiveSleepTimeCannotBeZero);
                }
                this.inactiveSleepTime = value;
            }
        }

        public bool IsActive
        {
            get
            {
                bool isVisible = false;
                if (GamerServicesDispatcher.IsInitialized)
                {
                    isVisible = Guide.IsVisible;
                }
                return (this.isActive && !isVisible);
            }
        }

        internal bool IsActiveIgnoringGuide
        {
            get
            {
                return this.isActive;
            }
        }

        public bool IsFixedTimeStep
        {
            get
            {
                return this.isFixedTimeStep;
            }
            set
            {
                this.isFixedTimeStep = value;
            }
        }

        public bool IsMouseVisible
        {
            get
            {
                return this.isMouseVisible;
            }
            set
            {
                this.isMouseVisible = value;
                if (this.Window != null)
                {
                    this.Window.IsMouseVisible = value;
                }
            }
        }

        public LaunchParameters LaunchParameters
        {
            get
            {
                return this.launchParameters;
            }
        }

        public GameServiceContainer Services
        {
            get
            {
                return this.gameServices;
            }
        }

        private bool ShouldExit
        {
            get
            {
                return this.exitRequested;
            }
        }

        public TimeSpan TargetElapsedTime
        {
            get
            {
                return this.targetElapsedTime;
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("value", Resources.TargetElaspedCannotBeZero);
                }
                this.targetElapsedTime = value;
            }
        }

        public GameWindow Window
        {
            get
            {
                if (this.host != null)
                {
                    return this.host.Window;
                }
                return null;
            }
        }
    }
*/
}
