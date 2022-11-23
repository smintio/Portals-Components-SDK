using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Impl;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.ExternalUsers;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.ExternalUsers.Models;
using SmintIo.Portals.DataAdapterSDK.DataAdapters.Interfaces.ExternalUsers.Results;

namespace SmintIo.Portals.DataAdapter.Picturepark.ExternalUsers
{
    public partial class PictureparkExternalUsersDataAdapter : DataAdapterBaseImpl, IExternalUsersRead
    {
        public async Task<GetUserGroupMembershipResult> GetUserGroupMembershipAsync(GetUserGroupMembershipParameters parameters)
        {
            var profileUserRoleIds = await _client.GetProfileUserRoleIdsAsync().ConfigureAwait(false);

            if (profileUserRoleIds == null)
            {
                return null;
            }

            if (!profileUserRoleIds.Any())
            {
                return new GetUserGroupMembershipResult()
                {
                    ExternalUserGroups = new List<ExternalUserGroupModel>()
                };
            }

            return new GetUserGroupMembershipResult()
            {
                ExternalUserGroups = profileUserRoleIds.Select(userRoleId => new ExternalUserGroupModel(Context)
                {
                    Id = userRoleId
                }).ToList()
            };
        }
    }
}