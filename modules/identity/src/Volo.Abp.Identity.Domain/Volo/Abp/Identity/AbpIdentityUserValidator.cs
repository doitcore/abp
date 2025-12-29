using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;
using Volo.Abp.DistributedLocking;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.Identity
{
    public class AbpIdentityUserValidator : IUserValidator<IdentityUser>
    {
        protected IdentityErrorDescriber ErrorDescriber { get; }
        protected IOptions<AbpMultiTenancyOptions> MultiTenancyOptions { get; }
        protected IAbpDistributedLock DistributedLock { get; }
        protected ICurrentTenant CurrentTenant { get; }
        protected IDataFilter<IMultiTenant> TenantFilter { get; }
        protected IIdentityUserRepository UserRepository { get; }
        protected IUserValidator<IdentityUser> DefaultUserValidator { get; }

        public AbpIdentityUserValidator(
            IdentityErrorDescriber errorDescriber,
            IOptions<AbpMultiTenancyOptions> multiTenancyOptions,
            IAbpDistributedLock distributedLock,
            ICurrentTenant currentTenant,
            IDataFilter<IMultiTenant> tenantFilter,
            IIdentityUserRepository userRepository)
        {
            ErrorDescriber = errorDescriber;
            MultiTenancyOptions = multiTenancyOptions;
            DistributedLock = distributedLock;
            CurrentTenant = currentTenant;
            TenantFilter = tenantFilter;
            UserRepository = userRepository;
            DefaultUserValidator = new UserValidator<IdentityUser>(ErrorDescriber);
        }

        public virtual async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user)
        {
            Check.NotNull(manager, nameof(manager));
            Check.NotNull(user, nameof(user));

            return MultiTenancyOptions.Value.UserSharingStrategy == TenantUserSharingStrategy.Isolated
                ? await ValidateIsolatedUserAsync(manager, user)
                : await ValidateSharedUserAsync(manager, user);
        }

        protected virtual async Task<IdentityResult> ValidateIsolatedUserAsync(UserManager<IdentityUser> manager, IdentityUser user)
        {
            var errors = new List<IdentityError>();

            var defaultValidationResult = await DefaultUserValidator.ValidateAsync(manager, user);
            if (!defaultValidationResult.Succeeded)
            {
                return defaultValidationResult;
            }

            var userName = await manager.GetUserNameAsync(user);
            if (userName == null)
            {
                errors.Add(ErrorDescriber.InvalidUserName(null));
            }
            else
            {
                var owner = await manager.FindByEmailAsync(userName);
                if (owner != null && !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)))
                {
                    errors.Add(ErrorDescriber.InvalidUserName(userName));
                }
            }

            var email = await manager.GetEmailAsync(user);
            if (email == null)
            {
                errors.Add(ErrorDescriber.InvalidEmail(null));
            }
            else
            {
                var owner = await manager.FindByNameAsync(email);
                if (owner != null && !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)))
                {
                    errors.Add(ErrorDescriber.InvalidEmail(email));
                }
            }

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        protected virtual async Task<IdentityResult> ValidateSharedUserAsync(UserManager<IdentityUser> manager, IdentityUser user)
        {
            var errors = new List<IdentityError>();

            var defaultValidationResult = await BuiltInValidateAsync(manager, user);
            if (!defaultValidationResult.Succeeded)
            {
                return defaultValidationResult;
            }

            await using var handle = await DistributedLock.TryAcquireAsync(nameof(AbpIdentityUserValidator), TimeSpan.FromMinutes(1));
            if (handle == null)
            {
                throw new AbpException("Could not acquire distributed lock for validating user uniqueness for shared user sharing strategy!");
            }

            using (CurrentTenant.Change(null))
            {
                using (TenantFilter.Disable())
                {
                    var owner = await manager.FindByIdAsync(user.Id.ToString());
                    var normalizedUserName = manager.NormalizeName(user.UserName);
                    var normalizedEmail = manager.NormalizeEmail(user.Email);
                    var users = await UserRepository.GetUsersByNormalizedUserNamesAsync([normalizedUserName!, normalizedEmail], true);
                    users.RemoveAll(x => x.Id == user.Id);
                    if (owner != null)
                    {
                        users.RemoveAll(x => x.NormalizedUserName == user.NormalizedUserName || x.NormalizedEmail == user.NormalizedEmail);
                    }
                    if (users.Any())
                    {
                        var userNames = users.Select(u => u.UserName).ToList();
                        errors.Add(userNames.Contains(user.UserName) ? ErrorDescriber.InvalidUserName(user.UserName!) : ErrorDescriber.InvalidEmail(user.Email!));
                    }

                    users = await UserRepository.GetUsersByNormalizedEmailsAsync([normalizedUserName!, normalizedEmail], true);
                    users.RemoveAll(x => x.Id == user.Id);
                    if (owner != null)
                    {
                        users.RemoveAll(x => x.NormalizedUserName == user.NormalizedUserName || x.NormalizedEmail == user.NormalizedEmail);
                    }
                    if (users.Any())
                    {
                        var emails = users.Select(u => u.Email).ToList();
                        errors.Add(emails.Contains(user.Email) ? ErrorDescriber.InvalidEmail(user.Email!) : ErrorDescriber.InvalidUserName(user.UserName!));
                    }
                }
            }

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        public virtual async Task<IdentityResult> BuiltInValidateAsync(UserManager<IdentityUser> manager, IdentityUser user)
        {
            var errors = await ValidateUserName(manager, user);
            if (manager.Options.User.RequireUniqueEmail)
            {
                errors.AddRange(await ValidateEmail(manager, user));
            }
            return errors?.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private async Task<List<IdentityError>> ValidateUserName(UserManager<IdentityUser> manager, IdentityUser user)
        {
            var errors = new List<IdentityError>();
            var userName = await manager.GetUserNameAsync(user);
            if (string.IsNullOrWhiteSpace(userName))
            {
                errors.Add(ErrorDescriber.InvalidUserName(userName));
            }
            else if (!string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
                userName.Any(c => !manager.Options.User.AllowedUserNameCharacters.Contains(c)))
            {
                errors.Add(ErrorDescriber.InvalidUserName(userName));
            }
            else
            {
                var owner = await manager.FindByNameAsync(userName);
                if (owner != null &&
                    !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)) &&
                    owner.TenantId == user.TenantId)
                {
                    errors.Add(ErrorDescriber.DuplicateUserName(userName));
                }
            }

            return errors;
        }

        // make sure email is not empty, valid, and unique
        private async Task<List<IdentityError>> ValidateEmail(UserManager<IdentityUser> manager, IdentityUser user)
        {
            var errors = new List<IdentityError>();
            var email = await manager.GetEmailAsync(user);
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(ErrorDescriber.InvalidEmail(email));
                return errors;
            }
            if (!new EmailAddressAttribute().IsValid(email))
            {
                errors.Add(ErrorDescriber.InvalidEmail(email));
                return errors;
            }
            var owner = await manager.FindByEmailAsync(email);
            if (owner != null &&
                !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)) &&
                owner.TenantId == user.TenantId)
            {
                errors.Add(ErrorDescriber.DuplicateEmail(email));
            }
            return errors;
        }
    }
}
