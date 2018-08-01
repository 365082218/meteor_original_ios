//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Event;
using Outfit7.Threading;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Grid {

    /// <summary>
    /// Grid data manager.
    /// </summary>
    public class GridManager {

        private const string Tag = "GridManager";

        public EventBus EventBus { get; set; }

        public GridPlugin GridPlugin { get; set; }

        public JSONNode JsonData { get; private set; }

        public void Setup() {
            O7Log.DebugT(Tag, "Setup");

            string jsonData = GridPlugin.LoadGridData();
            if (jsonData == null) {
                O7Log.WarnT(Tag, "No GRID data. First run?");
                return;
            }

            JsonData = Parse(jsonData);
        }

        internal void OnFreshDataDownload(string jsonData) {
            Executor executor = new Executor();
            Action runner = delegate {
                JSONNode gridJ = Parse(jsonData);

                executor.Post(delegate {
                    JsonData = gridJ;
                    if (Ready) {
                        EventBus.FireEvent(CommonEvents.FRESH_GRID_DOWNLOAD, JsonData);
                    }
                });
            };
            SimpleWorker.RunAsync(runner);
        }

        // May not be on main thread!
        private JSONNode Parse(string jsonData) {
            O7Log.DebugT(Tag, "Parsing GRID data...");

            JSONNode gridJ;
            try {
                gridJ = JSON.Parse(jsonData);

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot parse GRID data");
                return null;
            }

            O7Log.DebugT(Tag, "Parsed GRID data");

            return gridJ;
        }

        public void UpdateNewsTimestamp(DateTime time) {
            GridPlugin.UpdateNewsTimestamp(time);
        }

        public bool Ready {
            get {
                return JsonData != null;
            }
        }
    }
}
