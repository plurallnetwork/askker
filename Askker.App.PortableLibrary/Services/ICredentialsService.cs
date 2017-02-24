using Askker.App.PortableLibrary.Models;
using System;

namespace Askker.App.PortableLibrary.Services
{
    public interface ICredentialsService
    {
        string access_token { get; }

        string token_type { get; }

        int expires_in { get; }

        DateTime issued { get; }

        DateTime expires { get; }

        string userName { get;  }

        string name { get; }

        string id { get; }

        string profilePicturePath { get; }

        bool isShowingTour { get; }

        bool isStillValid { get; }

        void SaveCredentials(TokenModel model);

        void DeleteCredentials();

        bool DoCredentialsExist();
    }
}
