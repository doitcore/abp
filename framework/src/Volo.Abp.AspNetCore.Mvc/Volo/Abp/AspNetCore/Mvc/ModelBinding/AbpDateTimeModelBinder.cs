using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Volo.Abp.Timing;

namespace Volo.Abp.AspNetCore.Mvc.ModelBinding;

public class AbpDateTimeModelBinder : IModelBinder
{
    private readonly DateTimeModelBinder _dateTimeModelBinder;
    private readonly IClock _clock;

    public AbpDateTimeModelBinder(IClock clock, DateTimeModelBinder dateTimeModelBinder)
    {
        _clock = clock;
        _dateTimeModelBinder = dateTimeModelBinder;
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        await _dateTimeModelBinder.BindModelAsync(bindingContext);
    
        if (!bindingContext.Result.IsModelSet || bindingContext.Result.Model is not DateTime dateTime)
        {
            return;
        }
    
        // If the DateTime has no timezone info (most cases from input)
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            // Try to get user's timezone
            var userTz = _currentTimezoneProvider.TimeZone;
            if (!userTz.IsNullOrWhiteSpace())
            {
                try
                {
                    var tzInfo = _timezoneProvider.GetTimeZoneInfo(userTz);
                    // Treat the input as user's local time and convert to UTC
                    var utc = TimeZoneInfo.ConvertTimeToUtc(dateTime, tzInfo);
                    bindingContext.Result = ModelBindingResult.Success(utc);
                    return;
                }
                catch
                {
                    // fallback to default clock normalization if invalid TZ
                }
            }
        }
    
        // fallback: original behavior
        bindingContext.Result = ModelBindingResult.Success(_clock.Normalize(dateTime));
    }
}
