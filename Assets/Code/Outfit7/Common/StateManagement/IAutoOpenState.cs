//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

namespace Outfit7.StateManagement {
    public interface IAutoOpenState {
        bool CanOpen() ;

        bool AutoClear();
    }
}

