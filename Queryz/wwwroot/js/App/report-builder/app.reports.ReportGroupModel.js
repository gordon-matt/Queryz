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

        $("#Grid").kendoGrid({
            data: null,
            dataSource: {
                type: "odata",
                transport: {
                    read: {
                        url: reportGroupApiUrl,
                        dataType: "json"
                    },
                    parameterMap: function (options, operation) {
                        let paramMap = kendo.data.transports.odata.parameterMap(options);
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
                },
                schema: {
                    data: function (data) {
                        return data.value;
                    },
                    total: function (data) {
                        return data["@odata.count"];
                    },
                    model: {
                        id: "Id",
                        fields: {
                            Name: { type: "string" }
                        }
                    }
                },
                pageSize: this.parent.gridPageSize,
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true,
                sort: { field: "Name", dir: "asc" }
            },
            dataBound: function (e) {
                let body = this.element.find("tbody")[0];
                if (body) {
                    ko.cleanNode(body);
                    ko.applyBindings(ko.dataFor(body), body);
                }
                this.expandRow(this.tbody.find("tr.k-master-row").first());
            },
            filterable: true,
            sortable: {
                allowUnsort: false
            },
            pageable: {
                refresh: true
            },
            scrollable: false,
            columns: [{
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
                            '<a data-bind="click: reportGroupModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + this.parent.translations.edit + '">' +
                            '<i class="fas fa-edit"></i></a>' +

                            '<a data-bind="click: reportGroupModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + this.parent.translations.delete + '">' +
                            '<i class="fas fa-xmark"></i></a>'
                    ) +
                    '<a data-bind="click: reportModel.create.bind($data,#=Id#)" class="btn btn-dark text-white" title="Create Report">' +
                    '<i class="fas fa-plus"></i></a>' +
                    '</div>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 180
            }],
            detailTemplate: kendo.template($("#reports-template").html()),
            detailInit: this.detailInit
        });
    };

    detailInit = (e) => {
        const groupId = e.data.Id;

        const detailRow = e.detailRow;

        detailRow.find(".tabstrip").kendoTabStrip({
            animation: {
                open: { effects: "fadeIn" }
            }
        });

        detailRow.find(".reports-grid").kendoGrid({
            data: null,
            dataSource: {
                type: "odata",
                transport: {
                    read: {
                        url: function (data) {
                            return `${reportGroupApiUrl}/Default.GetReports(groupId=${groupId})`;
                        },
                        dataType: "json"
                    },
                    parameterMap: function (options, operation) {
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
                    }
                },
                schema: {
                    data: function (data) {
                        return data.value;
                    },
                    total: function (data) {
                        return data["@odata.count"];
                    },
                    model: {
                        id: "Id",
                        fields: {
                            Name: { type: "string" }
                        }
                    }
                },
                pageSize: this.parent.gridPageSize,
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true,
                sort: { field: "Name", dir: "asc" },
            },
            dataBound: function (e) {
                let body = this.element.find("tbody")[0];
                if (body) {
                    ko.cleanNode(body);
                    ko.applyBindings(ko.dataFor(body), body);
                }
            },
            pageable: {
                refresh: true
            },
            scrollable: false,
            columns: [{
                field: "Name",
                title: this.parent.translations.columns.name,
                filterable: true
            }, {
                field: "Id",
                title: " ",
                template: !allowEdit ?
                    '<div class="btn-group">' +
                    '<a href="/report-builder/run-report/#=Id#" target="_blank" class="btn btn-success" title="Run Report">' +
                    '<i class="fas fa-play"></i></a>' +
                    '</div>' :

                    '<div class="btn-group">' +
                    '<a href="/report-builder/run-report/#=Id#" target="_blank" class="btn btn-success" title="Run Report">' +
                    '<i class="fas fa-play"></i></a>' +

                    '<a data-bind="click: reportModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + this.parent.translations.edit + '">' +
                    '<i class="fas fa-edit"></i></a>' +

                    '<a data-bind="click: reportModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + this.parent.translations.delete + '">' +
                    '<i class="fas fa-xmark"></i></a>' +
                    '</div>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 180
            }]
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
                        $('#Grid').data('kendoGrid').dataSource.read();
                        $('#Grid').data('kendoGrid').refresh();

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
                    $('#Grid').data('kendoGrid').dataSource.read();
                    $('#Grid').data('kendoGrid').refresh();

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
                    $('#Grid').data('kendoGrid').dataSource.read();
                    $('#Grid').data('kendoGrid').refresh();

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