using System;
using System.Collections.Generic;
using UnityEditor;

namespace Starlite.Raven {

    public static class RavenEditorCallbacks {

        /// <summary>
        /// Gets called whenever a property track needs to generate the right click context menu for a property event.
        /// </summary>
        public static Action<RavenPropertyTrackView, List<RavenPropertyTrackView.PropertyEventMenuData>, List<RavenPropertyTrackView.AnimationDataType>> e_GenerateCustomDataForPropertyEventMenus;

        /// <summary>
        /// Gets called whenever a property track needs to generate the right click context menu for adding new events onto the track.
        /// </summary>
        public static Action<GenericMenu, RavenPropertyTrackView> e_GenerateCustomEventsForPropertyTrackContextMenu;

        /// <summary>
        /// Gets called whenever an audio track needs to generate the right click context menu for adding new events onto the track.
        /// </summary>
        public static Action<GenericMenu, RavenAudioTrackView> e_GenerateCustomEventsForAudioTrackContextMenu;

        /// <summary>
        /// Gets called whenever a property event context menu opens. It remaps a specific menu entry's name into a different one.
        /// </summary>
        public static Func<RavenPropertyTrackView.PropertyEventMenuData, string> e_RemapCallback;
    }
}