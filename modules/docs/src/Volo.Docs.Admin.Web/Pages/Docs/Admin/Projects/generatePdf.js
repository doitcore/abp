var abp = abp || {};
$(function () {
    abp.modals.projectGeneratePdf = function () {

        var l = abp.localization.getResource('Docs');
        var projectAppService = volo.docs.projects.docsProject;
        var pdfGeneratorAppService = volo.docs.documents.docsDocumentPdfGenerator;
        var projectAdminAppService = volo.docs.admin.projectsAdmin;
        
        var initModal = function (publicApi, args) {
            
        };

        $("#Versions").change(function () {
            var shortname = $("#ShortName").val();
            var version = $("#Version").val();
            projectAppService.getLanguageList(shortname, version).done(function (result) {
                $("#Language").empty();
                $.each(result.languages, function (index, item) {
                    $("#Language").append($(`<option value="${item.code}">${item.displayName}</option>`));
                });
            });
        })
        
        $("#GenerateBtn").click(function () {
            var $btn = $(this);
            $btn.buttonBusy(true);
            $("#GenerateAndDownloadPdfBtn").buttonBusy(true);
            var input = {
                projectId: $("#ProjectId").val(),
                version: $("#Version").val(),
                languageCode: $("#Language").val(),
            }
            
            function generatePdf(input) {
                pdfGeneratorAppService.generatePdf(input, {
                    abpHandleError : false,
                    error: function (jqXHR) {
                        if (jqXHR.status === 200) {
                            abp.message.success(l('PdfFileGeneratedSuccessfully'));
                            $btn.buttonBusy(false);
                            $("#GenerateAndDownloadPdfBtn").buttonBusy(false);
                        } else {
                            abp.ajax.handleErrorStatusCode(jqXHR.status);
                        }
                    }
                });
            }
            if(shouldForceToGenerate(input)){
                projectAdminAppService.deletePdfFile(input).done(() =>{
                    generatePdf(input);
                });
            }else{
                generatePdf(input);
            }
        })
        
        $("#GenerateAndDownloadPdfBtn").click(function () {
            var input = {
                projectId: $("#ProjectId").val(),
                version: $("#Version").val(),
                languageCode: $("#Language").val(),
            }
            if(shouldForceToGenerate(input)){
                projectAdminAppService.deletePdfFile(input).done(() =>{
                    window.open(abp.appPath + 'api/docs/documents/pdf' + abp.utils.buildQueryString([{ name: 'projectId', value: input.projectId }, { name: 'version', value: input.version }, { name: 'languageCode', value: input.languageCode }]), '_blank');
                });
            }else{
                window.open(abp.appPath + 'api/docs/documents/pdf' + abp.utils.buildQueryString([{ name: 'projectId', value: input.projectId }, { name: 'version', value: input.version }, { name: 'languageCode', value: input.languageCode }]), '_blank');
            }
        })
        
        function shouldForceToGenerate(input) {
            return $("#ForceToGenerate").is(":checked");
        }
        
        return {
            initModal: initModal,
        };
    };
});
