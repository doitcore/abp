var abp = abp || {};
(function ($) {
    var $all = $("#grantAllresourcePermissions");
    var $items = $("#permissionList input[type='checkbox']").not("#grantAllresourcePermissions");
    $all.on("change", function () {
        $items.prop("checked", this.checked);
    });
    $items.on("change", function () {
        $all.prop("checked", $items.length === $items.filter(":checked").length);
    });

    var $providerKey = $("#AddModel_ProviderKey");
    $providerKey.select2({
        ajax: {
            url: '/api/permission-management/permissions/search-resource-provider-keys',
            delay: 250,
            dataType: "json",
            data: function (params) {
                var query = {};
                query["serviceName"] = $('input[name="AddModel.ProviderName"]:checked').val();
                query["filter"] = params.term;
                return query;
            },
            processResults: function (data) {
                var keyValues = [];
                data.keys.forEach(function (item, index) {
                    keyValues.push({
                        id: item["providerKey"],
                        text: item["providerDisplayName"],
                        displayName: item["providerDisplayName"]
                    })
                });
                return {
                    results: keyValues
                };
            }
        },
        width: '100%',
        dropdownParent: $('#addResourcePermissionManagementModal'),
        language: abp.localization.currentCulture.cultureName
    });

    $('input[name="AddModel.ProviderName"]').change(function () {
        $providerKey.val(null).trigger('change');
    });

    $('#addResourcePermissionManagementForm').submit(function () {
         $(this).valid();
    });

})(jQuery);
