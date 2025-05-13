var abp = abp || {};
$(function () {
    abp.modals.projectManagePdfFiles = function () {

        var l = abp.localization.getResource('Docs');
        var projectAdminAppService = volo.docs.admin.projectsAdmin;
        
        var _generatePdfModal = new abp.ModalManager({
            viewUrl: abp.appPath + 'Docs/Admin/Projects/GeneratePdf',
            modalClass: 'projectGeneratePdf',
        });
        
        var initModal = function (publicApi, args) {
            var _dataTable = $('#ProjectPdfFilesTable').DataTable(
                abp.libs.datatables.normalizeConfiguration({
                    processing: true,
                    serverSide: true,
                    scrollX: true,
                    paging: true,
                    searching: false,
                    autoWidth: false,
                    scrollCollapse: true,
                    order: [[2, 'desc']],
                    ajax: abp.libs.datatables.createAjax(
                        volo.docs.admin.projectsAdmin.getPdfFiles,
                        {
                            projectId : args.projectId
                        }
                    ),
                    columnDefs: [
                        {
                            rowAction: {
                                items: [
                                    {
                                        text: l('Delete'),
                                        confirmMessage: function (data) {
                                            return l('PdfFileDeletionWarningMessage', data.record.fileName);
                                        },
                                        action: function (data) {
                                            projectAdminAppService.deletePdfFile({
                                                projectId: data.record.projectId,
                                                version: data.record.version,
                                                languageCode: data.record.languageCode
                                            }).then(() => {
                                                _dataTable.ajax.reloadEx();
                                                abp.notify.success(l('PdfGeneratedSuccessfully'));
                                            })
                                        },
                                    }
                                ],
                            },
                        },
                        {
                            target: 1,
                            data: 'fileName',
                        },
                        {
                            target: 2,
                            data: 'version',
                        },
                        {
                            target: 3,
                            data: 'languageCode',
                        },
                        {
                            target: 4,
                            data: `creationTime`,
                            dataFormat: "datetime"
                        },
                        {
                            target: 5,
                            data: `lastModificationTime`,
                            dataFormat: "datetime"
                        }
                    ],
                })
            );
            
            $('#GeneratePdfBtn').click(function () {
                _generatePdfModal.open({
                    Id: args.projectId,
                });
            });

            _generatePdfModal.onClose(function () {
                _dataTable.ajax.reloadEx();
            });
        };

        return {
            initModal: initModal,
        };
    };
});
