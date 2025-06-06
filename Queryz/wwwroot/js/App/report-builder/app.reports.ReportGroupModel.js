var ReportGroupModel = function (parent) {
    const self = this;
    self.parent = parent;

    self.id = ko.observable(0);
    self.name = ko.observable(null);
    self.timeZoneId = ko.observable(null);

    self.roles = ko.observableArray([]);

    self.validator = false;

    self.init = function () {
        self.validator = $("#report-group-form-section-form").validate({
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
                pageSize: self.parent.gridPageSize,
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
                title: self.parent.translations.columns.name,
                filterable: true
            }, {
                field: "Id",
                title: " ",
                template: !allowEdit ? ' ' :
                    '<div class="btn-group">' +
                    (
                        !isAdminUser ? ' ' :
                        '<a data-bind="click: reportGroupModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + self.parent.translations.edit + '">' +
                        '<i class="fas fa-edit"></i></a>' +

                        '<a data-bind="click: reportGroupModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + self.parent.translations.delete + '">' +
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
            detailInit: self.detailInit
        });
    };

    self.detailInit = function (e) {
        const groupId = e.data.Id;

        const detailRow = e.detailRow;

        detailRow.find(".tabstrip").kendoTabStrip({
            animation: {
                open: { effects: "fadeIn" }
            }
        });

        const detailGrid = detailRow.find(".reports-grid").kendoGrid({
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
                pageSize: self.parent.gridPageSize,
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
                title: self.parent.translations.columns.name,
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

                        '<a data-bind="click: reportModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + self.parent.translations.edit + '">' +
                        '<i class="fas fa-edit"></i></a>' +

                        '<a data-bind="click: reportModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + self.parent.translations.delete + '">' +
                        '<i class="fas fa-xmark"></i></a>' +
                    '</div>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 180
            }]
        });
    };
    self.create = function () {
        self.id(0);
        self.name(null);
        self.timeZoneId(null);

        self.roles([]);

        self.validator.resetForm();
        switchSection($("#report-group-form-section"));
        $("#report-group-form-section-legend").html(self.parent.translations.create);
    };
    self.edit = function (id) {
        $.ajax({
            url: `${reportGroupApiUrl}(${id})`,
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            self.id(json.Id);
            self.name(json.Name);
            self.timeZoneId(json.TimeZoneId);

            self.getRoles(id);

            self.validator.resetForm();
            switchSection($("#report-group-form-section"));
            $("#form-group-form-section-legend").html(self.parent.translations.edit);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.translations.getRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };
    self.remove = function (id) {
        if (confirm("If there are still reports assigned to this group, then deleting the group will also delete the reports. Are you sure you want to do that?")) {
            $.ajax({
                url: `${reportGroupApiUrl}(${id})`,
                type: "DELETE",
                async: false
            })
            .done(function (json) {
                $('#Grid').data('kendoGrid').dataSource.read();
                $('#Grid').data('kendoGrid').refresh();

                $.notify(self.parent.translations.deleteRecordSuccess, "success");
                self.parent.reportModel.step1.reloadGroups();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.deleteRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }
    };
    self.save = function () {
        const isNew = (self.id() == 0);

        if (!$("#report-group-form-section-form").valid()) {
            return false;
        }

        const record = {
            Id: self.id(),
            Name: self.name(),
            TimeZoneId: self.timeZoneId()
        };

        if (isNew) {
            $.ajax({
                url: reportGroupApiUrl,
                type: "POST",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(record),
                dataType: "json",
                async: false
            })
            .done(function (json) {
                $('#Grid').data('kendoGrid').dataSource.read();
                $('#Grid').data('kendoGrid').refresh();

                switchSection($("#grid-section"));

                self.id(json.Id);
                self.setRoles();

                $.notify(self.parent.translations.insertRecordSuccess, "success");
                self.parent.reportModel.step1.reloadGroups();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.insertRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }
        else {
            $.ajax({
                url: `${reportGroupApiUrl}(${self.id()})`,
                type: "PUT",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(record),
                dataType: "json",
                async: false
            })
            .done(function (json) {
                $('#Grid').data('kendoGrid').dataSource.read();
                $('#Grid').data('kendoGrid').refresh();

                switchSection($("#grid-section"));

                self.setRoles();
                
                $.notify(self.parent.translations.updateRecordSuccess, "success");
                self.parent.reportModel.step1.reloadGroups();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.updateRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }
    };
    self.cancel = function () {
        switchSection($("#grid-section"));
    };

    self.getRoles = function (id) {
        self.roles([]);
        $.ajax({
            url: `${reportGroupApiUrl}/Default.GetRoles(id=${id})`,
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.value && json.value.length > 0) {
                $.each(json.value, function () {
                    self.roles.push(this.Id);
                });
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify("Error when trying to retrieve roles for selected report group.", "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };

    self.setRoles = function () {
        $.ajax({
            url: `${reportGroupApiUrl}/Default.SetRoles`,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                id: self.id(),
                roles: self.roles()
            }),
            async: false
        })
        .done(function (json) {
            $.notify("Successfully saved roles for selected report group.", "success");
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify("Error when trying to save roles for selected report group.", "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };
};