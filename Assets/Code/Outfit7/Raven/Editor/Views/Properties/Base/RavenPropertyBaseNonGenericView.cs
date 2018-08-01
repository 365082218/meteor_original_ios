using UnityEngine;

namespace Starlite.Raven {

    public abstract class RavenPropertyBaseNonGenericView {

        public abstract void Initialize(RavenEventView eventView, RavenPropertyComponent property);

        public abstract void DrawGui(Rect position);

        public abstract void DrawExtendedGui(Rect position);

        public abstract bool HandleInput(Vector2 mousePosition);

        public abstract void RecordStart();

        public abstract void RecordEnd();
    }
}