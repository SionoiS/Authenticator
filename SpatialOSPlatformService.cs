using Google.Protobuf.WellKnownTypes;
using Improbable.SpatialOS.Deployment.V1Alpha1;
using Improbable.SpatialOS.Platform.Common;
using Improbable.SpatialOS.PlayerAuth.V2Alpha1;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Authenticator
{
    public interface ISpatialOSPlatform
    {
        Task<string> GetPlayerIdentityToken(string playerId);
        Task<string> GetLoginToken(string pit);
    }

    public class SpatialOSPlatformService : ISpatialOSPlatform
    {
        readonly DeploymentServiceClient deploymentServiceClient;
        readonly PlayerAuthServiceClient playerAuthServiceClient;

        const string ProjectName = "roguefleetonline";

        readonly string DeploymentTagFilter;

        public SpatialOSPlatformService()
        {
            var spatialOSServiceAccountToken = Environment.GetEnvironmentVariable("SPATIALOS_TOKEN");
            DeploymentTagFilter = Environment.GetEnvironmentVariable("DEPLOYMENT_TAG");

            var credentialsWithProvidedToken = new PlatformRefreshTokenCredential(spatialOSServiceAccountToken);

            deploymentServiceClient = DeploymentServiceClient.Create(credentials: credentialsWithProvidedToken);
            playerAuthServiceClient = PlayerAuthServiceClient.Create(credentials: credentialsWithProvidedToken);
        }

        public async Task<string> GetPlayerIdentityToken(string playerId)
        {
            var request = new CreatePlayerIdentityTokenRequest
            {
                Provider = "firebase_auth",
                PlayerIdentifier = playerId,
                ProjectName = ProjectName
            };

            var response = await playerAuthServiceClient.CreatePlayerIdentityTokenAsync(request);

            return response.PlayerIdentityToken;
        }

        public async Task<string> GetLoginToken(string pit)
        {
            var listDeploymentsRequest = new ListDeploymentsRequest
            {
                ProjectName = ProjectName,
                Filters = {new Filter
                {
                    TagsPropertyFilter = new TagsPropertyFilter
                    {
                        Tag = DeploymentTagFilter,
                        Operator = TagsPropertyFilter.Types.Operator.Equal,
                    },
                }}
            };

            var suitableDeployment = deploymentServiceClient.ListDeployments(listDeploymentsRequest).First();

            var loginRequest = new CreateLoginTokenRequest
            {
                PlayerIdentityToken = pit,
                DeploymentId = suitableDeployment.Id,
                LifetimeDuration = Duration.FromTimeSpan(new TimeSpan(0, 0, 15, 0)),
                WorkerType = "UnityClient"
            };

            var createLoginTokenResponse = await playerAuthServiceClient.CreateLoginTokenAsync(loginRequest);

            return createLoginTokenResponse.LoginToken;
        }
    }
}
