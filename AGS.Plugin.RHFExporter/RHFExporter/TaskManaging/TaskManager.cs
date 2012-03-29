using System;
using System.Collections.Generic;
using System.Text;

namespace RedHerringFarm.TaskManaging
{
    public class TaskManager : IDisposable
    {
        [ThreadStatic]
        private static TaskManager current = new TaskManager(null, "(Root task)", 1.0);

        public static TaskManager Current
        {
            get { return current; }
        }

        public readonly string Name;
        public readonly double TaskSize;
        private bool unpredictable = false;
        public bool Unpredictable
        {
            get { return unpredictable; }
        }

        private TaskManager parent;
        private TaskManager child;
        public TaskManager Parent
        {
            get
            {
                return parent;
            }
        }

        private TaskManager(TaskManager parent, string name, double taskSize)
        {
            this.parent = parent;
            Name = name;
            TaskSize = taskSize;
        }

        private Dictionary<string, TaskManager> expecting = new Dictionary<string,TaskManager>();
        private double expectingSize = 0.0;
        private double receivedSize = 0.0;

        public double RatioComplete
        {
            get
            {
                if (expectingSize == 0.0) return 1.0;
                double received = receivedSize;
                if (child != null) received += (child.RatioComplete * child.TaskSize);
                return (received / expectingSize);
            }
        }

        public int PercentComplete
        {
            get
            {
                return (int)(RatioComplete * 100.0);
            }
        }

        public static void Expect(string taskName)
        {
            Expect(taskName, 1.0);
        }
        public static void Expect(string taskName, double taskSize)
        {
            if (current.expecting.ContainsKey(taskName))
            {
                throw new Exception("Task is already expected");
            }
            current.expecting.Add(taskName, new TaskManager(current, taskName, taskSize));
            current.expectingSize += taskSize;
        }

        public event StatusUpdateEventHandler StatusUpdated;
        public event EventHandler Started;
        public event EventHandler Finished;

        public static void StatusUpdate(string update)
        {
            if (current.StatusUpdated != null)
            {
                current.StatusUpdated(current, new StatusUpdateEventArgs(update));
            }
        }

        public static TaskManager Start(string taskName)
        {
            return Start(taskName, false);
        }
        public static TaskManager Start(string taskName, bool unpredictable)
        {
            TaskManager newTaskManager;

            if (!current.expecting.TryGetValue(taskName, out newTaskManager))
            {
                throw new Exception("Not expecting task " + taskName);
            }
            current.expecting.Remove(taskName);

            newTaskManager.unpredictable = unpredictable;

            newTaskManager.StatusUpdated += current.ChildStatusUpdated;
            newTaskManager.Started += current.ChildStarted;
            newTaskManager.Finished += current.ChildFinished;

            current.child = newTaskManager;
            current = newTaskManager;

            newTaskManager.Started(newTaskManager, new EventArgs());

            return newTaskManager;
        }

        private void ChildStatusUpdated(object sender, StatusUpdateEventArgs e)
        {
            if (this.StatusUpdated != null)
            {
                this.StatusUpdated(sender, e);
            }
        }

        private void ChildStarted(object sender, EventArgs e)
        {
            if (this.Started != null)
            {
                this.Started(sender, e);
            }
        }

        private void ChildFinished(object sender, EventArgs e)
        {
            if (this.Finished != null)
            {
                this.Finished(sender, e);
            }
        }

        public void Dispose()
        {
            if (parent == null)
            {
                throw new Exception("Cannot dispose the root task");
            }
            parent.receivedSize += TaskSize;
            parent.child = null;
            current = parent;
            if (Finished != null)
            {
                Finished(this, new EventArgs());
            }
        }
    }

    public class StatusUpdateEventArgs
    {
        public StatusUpdateEventArgs(string text)
        {
            Text = text;
        }
        public readonly string Text;
    }
    public delegate void StatusUpdateEventHandler(object sender, StatusUpdateEventArgs e);

}
