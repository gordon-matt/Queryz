class ReportGroupModel {
    constructor(parent) {
        this.parent = parent;

        this.id = ko.observable(0);
        this.name = ko.observable(null);
        this.timeZoneId = ko.observable(null);
        this.roles = ko.observableArray([]);
        this.validator = false;
    }

    init = () => {
        this.validator = $("#report-group-form-section-form").validate({
            rules: {
                Name: { required: true, maxlength: 255 }
            }
        });

        GridHelper.initKendoDetailGrid(
            "Grid",
            reportGroupApiUrl,
            {
                id: "Id",
                fields: {
                    Name: { type: "string" }
                }
            }, [{
                field: "Name",
                title: this.parent.translations.columns.name,
                filterable: true
            }, {
                field: "Id",
                title: " ",
                template: !allowEdit ? ' ' :
                    '<div class="btn-group">' +
                    (
                        !isAdminUser ? ' ' :
                            GridHelper.actionIconButton("reportGroupModel.edit", 'fas fa-edit', this.parent.translations.edit, 'secondary', `#=Id#`) +
                            GridHelper.actionIconButton("reportGroupModel.remove", 'fas fa-xmark', this.parent.translations.delete, 'danger', `#=Id#`)
                    ) +
                    GridHelper.actionIconButton("reportModel.create", 'fas fa-plus', 'Create Report', 'secondary', `#=Id#`) +
                    '</div>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 180
            }],
            this.parent.gridPageSize,
            { field: "Name", dir: "asc" },
            null,
            kendo.template($("#reports-template").html()),
            this.detailInit);
    };

    detailInit = (e) => {
        const groupId = e.data.Id;

        const detailRow = e.detailRow;

        detailRow.find(".tabstrip").kendoTabStrip({
            animation: {
                open: { effects: "fadeIn" }
            }
        });

        GridHelper.initKendoGridByElement(
            detailRow.find(".reports-grid"),
            function (data) {
                return `${reportGroupApiUrl}/Default.GetReports(groupId=${groupId})`;
            },
            {
                id: "Id",
                fields: {
                    Name: { type: "string" }
                }
            }, [{
                field: "Name",
                title: this.parent.translations.columns.name,
                filterable: true
            }, {
                field: "Id",
                title: " ",
                template: !allowEdit ?
                    '<div class="btn-group">' +
                    '<a href="/report-builder/run-report/#=Id#" target="_blank" class="btn btn-success btn-sm" title="Run Report">' +
                    '<i class="fas fa-play"></i></a>' +
                    '</div>' :

                    '<div class="btn-group">' +
                    '<a href="/report-builder/run-report/#=Id#" target="_blank" class="btn btn-success btn-sm" title="Run Report">' +
                    '<i class="fas fa-play"></i></a>' +

                    GridHelper.actionIconButton("reportModel.edit", 'fas fa-edit', this.parent.translations.edit, 'secondary', `#=Id#`) +
                    GridHelper.actionIconButton("reportModel.remove", 'fas fa-xmark', this.parent.translations.delete, 'danger', `#=Id#`) +
                    '</div>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 180
            }],
            this.parent.gridPageSize,
            { field: "Name", dir: "asc" },
            function (options, operation) {
                if (operation === "read") {
                    let paramMap = kendo.data.transports.odata.parameterMap(options, operation);

                    if (paramMap.$inlinecount) {
                        if (paramMap.$inlinecount == "allpages") {
                            paramMap.$count = true;
                        }
                        delete paramMap.$inlinecount;
                    }
                    if (paramMap.$filter) {
                        paramMap.$filter = paramMap.$filter.replace(/substringof\((.+),(.*?)\)/, "contains($2,$1)");
                    }
                    return paramMap;
                }
                else {
                    return kendo.data.transports.odata.parameterMap(options, operation);
                }
            });
    };

    create = () => {
        this.id(0);
        this.name(null);
        this.timeZoneId(null);
        this.roles([]);

        this.validator.resetForm();
        switchSection($("#report-group-form-section"));
        $("#report-group-form-section-legend").html(this.parent.translations.create);
    };

    edit = (id) => {
        fetch(`${reportGroupApiUrl}(${id})`)
            .then(response => response.json())
            .then(json => {
                this.id(json.Id);
                this.name(json.Name);
                this.timeZoneId(json.TimeZoneId);

                this.getRoles(id);

                this.validator.resetForm();
                switchSection($("#report-group-form-section"));
                $("#form-group-form-section-legend").html(this.parent.translations.edit);
            })
            .catch(error => {
                $.notify(this.parent.translations.getRecordError, "error");
                console.error('Error: ', error);
            });
    };

    remove = (id) => {
        if (confirm("If there are still reports assigned to this group, then deleting the group will also delete the reports. Are you sure you want to do that?")) {
            fetch(`${reportGroupApiUrl}(${id})`, { method: 'DELETE' })
                .then(response => {
                    if (response.ok) {
                        GridHelper.refreshGrid();

                        $.notify(this.parent.translations.deleteRecordSuccess, "success");
                        this.parent.reportModel.step1.reloadGroups();
                    } else {
                        $.notify(this.parent.translations.deleteRecordError, "error");
                    }
                })
                .catch(error => {
                    $.notify(this.parent.translations.deleteRecordError, "error");
                    console.error('Error: ', error);
                });
        }
    };

    save = () => {
        const isNew = (this.id() == 0);

        if (!$("#report-group-form-section-form").valid()) {
            return false;
        }

        const record = {
            Id: this.id(),
            Name: this.name(),
            TimeZoneId: this.timeZoneId()
        };

        if (isNew) {
            fetch(reportGroupApiUrl, {
                method: "POST",
                headers: {
                    'Content-type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify(record)
            })
            .then(response => {
                if (response.ok) {
                    GridHelper.refreshGrid();

                    switchSection($("#grid-section"));

                    this.id(json.Id);
                    this.setRoles();

                    $.notify(this.parent.translations.insertRecordSuccess, "success");
                    this.parent.reportModel.step1.reloadGroups();
                } else {
                    $.notify(this.parent.translations.insertRecordError, "error");
                }
            })
            .catch(error => {
                $.notify(this.parent.translations.insertRecordError, "error");
                console.error('Error: ', error);
            });
        }
        else {
            fetch(reportGroupApiUrl, {
                method: "PUT",
                headers: {
                    'Content-type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify(record)
            })
            .then(response => {
                if (response.ok) {
                    GridHelper.refreshGrid();

                    switchSection($("#grid-section"));

                    this.setRoles();

                    $.notify(this.parent.translations.updateRecordSuccess, "success");
                    this.parent.reportModel.step1.reloadGroups();
                } else {
                    $.notify(this.parent.translations.updateRecordError, "error");
                }
            })
            .catch(error => {
                $.notify(this.parent.translations.updateRecordError, "error");
                console.error('Error: ', error);
            });
        }
    };

    cancel() {
        switchSection($("#grid-section"));
    };

    getRoles = (id) => {
        this.roles([]);
        fetch(`${reportGroupApiUrl}/Default.GetRoles(id=${id})`)
            .then(response => response.json())
            .then(json => {
                if (json.value?.length) {
                    json.value.forEach(item => this.roles.push(item.Id));
                }
            })
            .catch(error => {
                $.notify("Error when trying to retrieve roles for selected report group.", "error");
                console.error('Error: ', error);
            });
    };

    setRoles = () => {
        fetch(`${reportGroupApiUrl}/Default.SetRoles`, {
            method: "POST",
            headers: {
                'Content-type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify({
                id: this.id(),
                roles: this.roles()
            })
        })
        .then(response => {
            if (response.ok) {
                $.notify("Successfully saved roles for selected report group.", "success");
            } else {
                $.notify("Error when trying to save roles for selected report group.", "error");
            }
        })
        .catch(error => {
            $.notify("Error when trying to save roles for selected report group.", "error");
            console.error('Error: ', error);
        });
    };
}