using UnityEngine;

namespace Outfit7.Logic {

    public interface IManager {
        void SetInstance(MonoBehaviour manager);

        void OnCreate();

        bool OnInitialize();

        void OnTerminate();

        void OnPreUpdate(float deltaTime);

        void OnBucketUpdate(int index, float deltaTime);

        void OnPostUpdate(float deltaTime);

        void OnPause();

        void OnResume();
    };

}
