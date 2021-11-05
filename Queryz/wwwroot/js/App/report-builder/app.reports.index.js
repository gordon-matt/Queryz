'use strict'

var dataSourceApiUrl = "/odata/DataSourceApi";
var enumerationApiUrl = "/odata/EnumerationApi";
var reportGroupApiUrl = "/odata/ReportGroupApi";
var reportApiUrl = "/odata/ReportApi";

var DataSourceModel = function (parent) {
    var self = this;
    self.parent = parent;

    self.id = ko.observable(0);
    self.name = ko.observable(null);
    self.dataProvider = ko.observable(null);
    self.connectionDetails = ko.observable(null);

    self.validator = false;

    self.init = function () {
        self.validator = $("#report-group-form-section-form").validate({
            rules: {
                DataSource_Name: { required: true, maxlength: 255 }
            }
        });

        $("#DataSourceGrid").kendoGrid({
            data: null,
            dataSource: {
                type: "odata",
                transport: {
                    read: {
                        url: dataSourceApiUrl,
                        dataType: "json"
                    },
                    parameterMap: function (options, operation) {
                        var paramMap = kendo.data.transports.odata.parameterMap(options);
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
                var body = this.element.find("tbody")[0];
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
                template:
                    '<div class="btn-group">' +
                    '<a data-bind="click: dataSourceModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + self.parent.translations.edit + '">' +
                    '<i class="fal fa-edit"></i></a>' +

                    '<a data-bind="click: dataSourceModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + self.parent.translations.delete + '">' +
                    '<i class="fal fa-times"></i></a>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 150
            }]
        });
    };
    
    self.create = function () {
        self.id(0);
        self.name(null);
        self.dataProvider(null);
        self.connectionDetails(null);

        self.cleanUpConnectionDetails();
        
        self.validator.resetForm();
        switchSection($("#dataSources-form-section"));
        $("#dataSources-form-section-legend").html(self.parent.translations.create);
    };
    self.edit = function (id) {
        $.ajax({
            url: `${dataSourceApiUrl}(${id})`,
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            self.id(json.Id);
            self.name(json.Name);
            
            $.ajax({
                url: `/report-builder/get-connection-details/${id}`,
                type: "GET",
                dataType: "json",
                async: false
            })
            .done(function (json) {
                if (json.success) {
                    self.connectionDetails(json.connectionDetails);
                }
                else {
                    $.notify(json.message, "error");
                    console.log(json.message);
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.getRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });

            switch (json.DataProvider) {
                case 'SqlServer': self.dataProvider(0); break;
                case 'PostgreSql': self.dataProvider(1); break;
                case 'MySql': self.dataProvider(2); break;
                //default: $.notify(`Unknown data provider: '${json.DataProvider}'`, "error");
            }
            
            self.validator.resetForm();
            switchSection($("#dataSources-form-section"));
            $("#dataSources-form-section-legend").html(self.parent.translations.edit);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.translations.getRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };
    self.remove = function (id) {
        if (confirm(self.parent.translations.deleteRecordConfirm)) {
            $.ajax({
                url: `${dataSourceApiUrl}(${id})`,
                type: "DELETE",
                async: false
            })
            .done(function (json) {
                $('#DataSourceGrid').data('kendoGrid').dataSource.read();
                $('#DataSourceGrid').data('kendoGrid').refresh();

                $.notify(self.parent.translations.deleteRecordSuccess, "success");
                self.parent.reportModel.step1.reloadDataSources();
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.deleteRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }
    };
    self.save = function () {
        // ensure the function exists before calling it...
        if (typeof onBeforeSave == 'function') {
            onBeforeSave(self);
        }
        
        if (!$("#dataSources-form-section-form").valid()) {
            return false;
        }

        var record = {
            Id: self.id(),
            Name: self.name(),
            DataProvider: self.dataProvider(),
            ConnectionDetails: self.connectionDetails()
        };

        $.ajax({
            url: `${dataSourceApiUrl}/Default.Save`,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(record),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            $('#DataSourceGrid').data('kendoGrid').dataSource.read();
            $('#DataSourceGrid').data('kendoGrid').refresh();

            switchSection($("#dataSources-grid-section"));

            $.notify(self.parent.translations.insertRecordSuccess, "success");
            self.parent.reportModel.step1.reloadDataSources();
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.translations.insertRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };

    self.onDataProviderChanged = function () {
        var provider = self.dataProvider();

        if (provider == -1) {
            return false;
        }

        $.ajax({
            url: `/report-builder/get-connection-details-ui/${provider}`,
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.cleanUpConnectionDetails();

                var result = $(json.html);

                // Add new HTML
                var content = $(result.filter('#connection-details-content')[0]);
                var details = $('<div>').append(content.clone()).html();
                $("#connection-details").html(details);

                // Add new Scripts
                var scripts = result.filter('script');

                $.each(scripts, function () {
                    var script = $(this);
                    script.attr("data-settings-script", "true");
                    script.appendTo('body');
                });

                // Update Bindings
                // Ensure the function exists before calling it...
                if (typeof updateModel == 'function') {
                    var data = ko.toJS(ko.mapping.fromJSON(self.connectionDetails()));
                    updateModel(self, data);
                    var elementToBind = $("#connection-details")[0];
                    ko.applyBindings(self.parent, elementToBind);
                }
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.translations.GetRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };
    self.cleanUpConnectionDetails = function () {
        // Clean up from previously injected html/scripts
        if (typeof cleanUp === 'function') {
            cleanUp(self);
        }

        // Remove Old Scripts
        var oldScripts = $('script[data-settings-script="true"]');

        if (oldScripts.length > 0) {
            $.each(oldScripts, function () {
                $(this).remove();
            });
        }

        var elementToBind = $("#connection-details")[0];
        ko.cleanNode(elementToBind);
        $("#connection-details").html("");
    };

    self.cancel = function () {
        self.cleanUpConnectionDetails();
        switchSection($("#dataSources-grid-section"));
    };
    self.goBack = function () {
        self.cleanUpConnectionDetails();
        switchSection($("#grid-section"));
    };
};

var EnumerationModel = function (parent) {
    var self = this;
    self.parent = parent;

    self.id = ko.observable(0);
    self.name = ko.observable(null);
    self.isBitFlags = ko.observable(false);
    self.values = ko.observableArray([]);

    self.newValueId = 0; // A temporary value, auto-incremented.. used for automatically setting the next value. Can still be changed by user, if needed.

    self.validator = false;

    self.init = function () {
        self.validator = $("#enumerations-form-section-form").validate({
            rules: {
                Enumeration_Name: { required: true, maxlength: 128 }
            }
        });

        $("#EnumerationsGrid").kendoGrid({
            data: null,
            dataSource: {
                type: "odata",
                transport: {
                    read: {
                        url: enumerationApiUrl,
                        dataType: "json"
                    },
                    parameterMap: function (options, operation) {
                        var paramMap = kendo.data.transports.odata.parameterMap(options);
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
                var body = this.element.find("tbody")[0];
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
                template:
                    '<div class="btn-group">' +
                        '<a data-bind="click: enumerationModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + self.parent.translations.edit + '">' +
                        '<i class="fal fa-edit"></i></a>' +

                        '<a data-bind="click: enumerationModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + self.parent.translations.delete + '">' +
                        '<i class="fal fa-times"></i></a>' +
                    '</div>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 150
            }]
        });
    };
    
    self.create = function () {
        self.id(0);
        self.name(null);
        self.isBitFlags(false);
        self.values([]);
        self.newValueId = 0;
        
        self.validator.resetForm();
        switchSection($("#enumerations-form-section"));
        $("#enumerations-form-section-legend").html(self.parent.translations.create);
    };
    self.edit = function (id) {
        self.values([]);
        self.newValueId = 0;

        $.ajax({
            url: `${enumerationApiUrl}(${id})`,
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            self.id(json.Id);
            self.name(json.Name);
            self.isBitFlags(json.IsBitFlags);

            if (json.Values) {
                var data = ko.toJS(ko.mapping.fromJSON(json.Values));
                $.each(data, function () {
                    self.values.push(this);
                    self.newValueId = this.id + 1;
                });
            }

            self.validator.resetForm();
            switchSection($("#enumerations-form-section"));
            $("#enumerations-form-section-legend").html(self.parent.translations.edit);
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.translations.getRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };
    self.remove = function (id) {
        if (confirm(self.parent.translations.deleteRecordConfirm)) {
            $.ajax({
                url: `${enumerationApiUrl}(${id})`,
                type: "DELETE",
                async: false
            })
            .done(function (json) {
                $('#EnumerationsGrid').data('kendoGrid').dataSource.read();
                $('#EnumerationsGrid').data('kendoGrid').refresh();

                $.notify(self.parent.translations.deleteRecordSuccess, "success");
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.deleteRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }
    };
    self.save = function () {
        var isNew = (self.id() == 0);

        if (!$("#enumerations-form-section-form").valid()) {
            return false;
        }

        var valuesJson = ko.mapping.toJSON(self.values());

        var record = {
            Id: self.id(),
            Name: self.name(),
            IsBitFlags: self.isBitFlags(),
            Values: valuesJson
        };

        if (isNew) {
            $.ajax({
                url: enumerationApiUrl,
                type: "POST",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(record),
                dataType: "json",
                async: false
            })
            .done(function (json) {
                $('#EnumerationsGrid').data('kendoGrid').dataSource.read();
                $('#EnumerationsGrid').data('kendoGrid').refresh();

                switchSection($("#enumerations-grid-section"));

                $.notify(self.parent.translations.insertRecordSuccess, "success");
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.insertRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }
        else {
            $.ajax({
                url: `${enumerationApiUrl}(${self.id()})`,
                type: "PUT",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(record),
                dataType: "json",
                async: false
            })
            .done(function (json) {
                $('#EnumerationsGrid').data('kendoGrid').dataSource.read();
                $('#EnumerationsGrid').data('kendoGrid').refresh();

                switchSection($("#enumerations-grid-section"));

                $.notify(self.parent.translations.updateRecordSuccess, "success");
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.updateRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }
    };

    self.cancel = function () {
        switchSection($("#enumerations-grid-section"));
    };
    self.goBack = function () {
        switchSection($("#grid-section"));
    };
    
    self.addEnumerationValue = function () {
        self.values.push({ id: self.newValueId, name: null });
        self.newValueId++;
    };
    self.removeEnumerationValue = function () {
        self.values.remove(this);
    };
};

var ReportGroupModel = function (parent) {
    var self = this;
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
                        var paramMap = kendo.data.transports.odata.parameterMap(options);
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
                var body = this.element.find("tbody")[0];
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
                        '<i class="fal fa-edit"></i></a>' +

                        '<a data-bind="click: reportGroupModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + self.parent.translations.delete + '">' +
                        '<i class="fal fa-times"></i></a>'
                    ) +
                        '<a data-bind="click: reportModel.create.bind($data,#=Id#)" class="btn btn-dark text-white" title="Create Report">' +
                        '<i class="fal fa-plus"></i></a>' +
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
        var groupId = e.data.Id;

        var detailRow = e.detailRow;

        detailRow.find(".tabstrip").kendoTabStrip({
            animation: {
                open: { effects: "fadeIn" }
            }
        });

        var detailGrid = detailRow.find(".reports-grid").kendoGrid({
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
                            var paramMap = kendo.data.transports.odata.parameterMap(options, operation);

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
                var body = this.element.find("tbody")[0];
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
                        '<i class="fal fa-play"></i></a>' +
                    '</div>' :

                    '<div class="btn-group">' +
                        '<a href="/report-builder/run-report/#=Id#" target="_blank" class="btn btn-success" title="Run Report">' +
                        '<i class="fal fa-play"></i></a>' +

                        '<a data-bind="click: reportModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + self.parent.translations.edit + '">' +
                        '<i class="fal fa-edit"></i></a>' +

                        '<a data-bind="click: reportModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + self.parent.translations.delete + '">' +
                        '<i class="fal fa-times"></i></a>' +
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
        var isNew = (self.id() == 0);

        if (!$("#report-group-form-section-form").valid()) {
            return false;
        }

        var record = {
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

var SelectedColumnModel = function (columnName, type, alias, isLiteral, displayColumn, enumerationId, transformFunction, format, isHidden) {
    var self = this;

    self.columnName = columnName;
    self.type = ko.observable(type);
    self.alias = ko.observable(alias);
    self.isLiteral = isLiteral;
    self.displayColumn = displayColumn;
    self.enumerationId = enumerationId;
    self.transformFunction = transformFunction;
    self.format = format;
    self.isHidden = isHidden;
};

var RelationshipModel = function (tableName, parentTable, primaryKey, foreignKey, joinType, availableColumns, availableParentTables) {
    var self = this;

    self.tableName = tableName;
    self.parentTable = parentTable;
    self.primaryKey = primaryKey; // Why does this keep disappearing??? Is it a reseverd word?
    self.foreignKey = foreignKey;
    self.joinType = joinType;
    self.availableColumns = availableColumns;
    self.availableParentTables = availableParentTables;

    self.primaryKeyBackup = primaryKey; // Have extra primary Key field because other one keeps disappearing.
};

var WizardStep1Model = function (parent) {
    var self = this;
    self.parent = parent;

    self.id = ko.observable(0);
    self.name = ko.observable(null);
    self.groupId = ko.observable(0);
    self.dataSourceId = ko.observable(0);
    self.enabled = ko.observable(false);
    self.emailEnabled = ko.observable(false);

    self.validator = $("#wizard-form-1").validate();

    self.availableGroups = ko.observableArray([]);
    self.availableDataSources = ko.observableArray([]);

    self.save = function () {
        if (!$("#wizard-form-1").valid()) {
            return false;
        }

        var success = false;

        $.ajax({
            url: "/report-builder/save-wizard-step-1",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                Id: self.id(),
                Name: self.name(),
                GroupId: self.groupId(),
                DataSourceId: self.dataSourceId(),
                Enabled: self.enabled(),
                EmailEnabled: self.emailEnabled()
            }),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step2.reportId(json.model.reportId);
                self.parent.step2.availableTables([]);
                self.parent.step2.selectedTables([]);
                $.each(json.model.availableTables, function () {
                    self.parent.step2.availableTables.push(this);
                });
                $.each(json.model.selectedTables, function () {
                    self.parent.step2.selectedTables.push(this);
                });

                success = true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                success = false;
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.parent.translations.UpdateRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
            success = false;
        });

        return success;
    };

    self.reloadGroups = function () {
        // We use timeout to prevent error when both the grid and this try to call the same url at the same time. It causes
        //  a NotSupportedException on the db context
        setTimeout(function () {
            $.ajax({
                url: reportGroupApiUrl,
                type: "GET",
                dataType: "json",
                async: false
            })
            .done(function (json) {
                self.availableGroups([]);
                $.each(json.value, function () {
                    var group = this;
                    self.availableGroups.push({ id: group.Id, name: group.Name });
                });
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify("Could not reload groups", "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }, 1000);
    };

    self.reloadDataSources = function () {
        // We use timeout to prevent error when both the grid and this try to call the same url at the same time. It causes
        //  a NotSupportedException on the db context
        setTimeout(function () {
            $.ajax({
                url: `${dataSourceApiUrl}?$orderby=Name`,
                type: "GET",
                dataType: "json",
                async: false
            })
            .done(function (json) {
                self.availableDataSources([]);
                $.each(json.value, function () {
                    var dataSource = this;
                    self.availableDataSources.push({ id: dataSource.Id, name: dataSource.Name });
                });
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify("Could not reload data sources", "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }, 1000);
    };
};

var WizardStep2Model = function (parent) {
    var self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.availableTables = ko.observableArray([]);
    self.selectedTables = ko.observableArray([]);

    self.save = function () {
        var success = false;

        $.ajax({
            url: "/report-builder/save-wizard-step-2",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            //data: JSON.stringify($('#wizard-form-2').serializeObject()),
            data: ko.toJSON({
                ReportId: self.reportId,
                SelectedTables: self.selectedTables
            }),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step3.reportId(json.model.reportId);
                self.parent.step3.availableColumns([]);
                self.parent.step3.selectedColumns([]);
                self.parent.step3.availableEnumerations([]);
                self.parent.step3.availableTransformFunctions([]);

                $.each(json.model.availableColumns, function () {
                    var column = this;
                    self.parent.step3.availableColumns.push({
                        name: column.columnName,
                        type: column.type,
                        isForeignKey: column.isForeignKey,
                        availableParentColumns: column.availableParentColumns
                    });
                });

                $.each(json.model.availableEnumerations, function (i, val) {
                    self.parent.step3.availableEnumerations.push({ id: val.id, name: val.name });
                });

                $.each(json.model.availableTransformFunctions, function () {
                    var transformFunction = this;
                    self.parent.step3.availableTransformFunctions.push({ name: transformFunction });
                });

                $.each(json.model.selectedColumns, function (i, selectedColumn) {
                    self.parent.step3.selectedColumns.push(new SelectedColumnModel(
                        selectedColumn.columnName,
                        selectedColumn.type,
                        selectedColumn.alias,
                        selectedColumn.isLiteral,
                        selectedColumn.displayColumn,
                        selectedColumn.enumerationId,
                        selectedColumn.transformFunction,
                        selectedColumn.format,
                        selectedColumn.isHidden
                    ));
                });

                success = true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                success = false;
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.parent.translations.UpdateRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
            success = false;
        });

        return success;
    };
};

var WizardStep3Model = function (parent) {
    var self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.availableColumns = ko.observableArray([]);
    self.selectedColumns = ko.observableArray([]);
    self.availableEnumerations = ko.observableArray([]);
    self.availableTransformFunctions = ko.observableArray([]);

    self.validator = $("#wizard-form-3").validate();
    
    self.addColumn = function () {
        self.selectedColumns.push(new SelectedColumnModel(null, null, null, false, null, null, null, null, false));
        var index = self.selectedColumns().length - 1;
        self.column_onChange(index);
    };
    self.addLiteral = function () {
        self.selectedColumns.push(new SelectedColumnModel(null, null, null, true, null, null, null, null, false));
    };
    self.removeColumn = function () {
        self.selectedColumns.remove(this);
    };

    self.removeAllColumns = function () {
        if (confirm("Are you sure you want to remove ALL selected columns?")) {
            self.selectedColumns([]);
        }
    };

    self.column_onChange = function (index) {
        var currentColumn = self.selectedColumns()[index];

        // Get column from availableColumns
        var col = ko.utils.arrayFirst(self.availableColumns(), function (item) {
            return item.name === currentColumn.columnName;
        });

        if (col == null) {
            return false;
        }

        currentColumn.type(col.type);
        
        var alias = col.name;
        var periodIndex = alias.indexOf('.');
        alias = alias.substring(periodIndex + 1);
        alias = titleCase(unCamelCase(alias));
        currentColumn.alias(alias);

        var select = $('select[name="SelectedColumns[' + index + '].DisplayColumn"]');
        select.find('option').remove();
        select.hide();

        if (col.isForeignKey && col.availableParentColumns && col.availableParentColumns.length > 0) {
            $(col.availableParentColumns).each(function (i, val) {
                select.append($('<option>', {
                    value: val,
                    text: val
                }));
            });
            select.show();
        }
    };

    self.save = function () {
        if (!$("#wizard-form-3").valid()) {
            return false;
        }

        // Ensure unique columns
        var nonLiteralColumns = $.grep(self.selectedColumns(), function (element) { return element.isLiteral == false; })
        var selectedColumns = nonLiteralColumns.map(function (col) {
            return col.columnName;
        });

        if (hasDuplicates(selectedColumns)) {
            $.notify('One or columns is selected multiple times. Please remove duplicates.', "error");
            console.log('One or columns is selected multiple times. Please remove duplicates.');
            return false;
        }

        var success = false;

        var formData = form2js('wizard-form-3', '.', true);

        $.ajax({
            url: "/report-builder/save-wizard-step-3",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(formData),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step4.init(json.model);
                success = true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                success = false;
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.parent.translations.UpdateRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
            success = false;
        });

        return success;
    };
};

var WizardStep4Model = function (parent) {
    var self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.query = ko.observable(null);

    self.id = -1;

    self.init = function (model) {
        self.reportId(model.reportId);
        self.query(model.query);
        
        $('#query-builder').queryBuilder('destroy');

        var queryBuilderConfig = model.jqQueryBuilderConfig;
        queryBuilderConfig.design_mode = true;

        $('#query-builder').queryBuilder(model.jqQueryBuilderConfig);

        $('#query-builder').on('getSQLFieldID.queryBuilder.filter', function (e) {
            if (e.value in model.jqQueryBuilderFieldIdMappings) {
                e.value = model.jqQueryBuilderFieldIdMappings[e.value];
            }
        });

        if (model.query) {
            try {
                //$('#query-builder').queryBuilder('setRulesFromSQL', model.query);
                $('#query-builder').queryBuilder('setRules', JSON.parse(model.query));
            }
            catch (x) {
                console.log(`Error when parsing query: ${model.query}.`)
                console.log(`The error message is as follows: ${x.message}.`)
            }
        }
    };
    
    self.save = function () {
        //var result = $('#query-builder').queryBuilder('getSQL', false);
        var result = JSON.stringify($('#query-builder').queryBuilder('getRules'));

        var success = false;
        
        $.ajax({
            url: "/report-builder/save-wizard-step-4",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                ReportId: self.reportId(),
                //Query: result.sql
                Query: result
            }),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step5.reportId(json.model.reportId);
                self.parent.step5.availableColumns([]);
                self.parent.step5.sortings([]);
                $.each(json.model.availableColumns, function () {
                    self.parent.step5.availableColumns.push(this);
                });
                $.each(json.model.sortings, function () {
                    var sorting = this;
                    self.parent.step5.sortings.push({
                        columnName: sorting.columnName,
                        sortDirection: sorting.sortDirection
                    });
                });

                success = true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                success = false;
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.parent.translations.UpdateRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
            success = false;
        });

        return success;
    };
};

var WizardStep5Model = function (parent) {
    var self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.availableColumns = ko.observableArray([]);
    self.sortings = ko.observableArray([]);

    self.validator = $("#wizard-form-5").validate();

    self.addSorting = function () {
        self.sortings.push({
            columnName: null,
            sortDirection: 0
        });
    };
    self.removeSorting = function () {
        self.sortings.remove(this);
    };

    self.save = function () {
        if (!$("#wizard-form-5").valid()) {
            return false;
        }

        var success = false;

        var formData = form2js('wizard-form-5', '.', true);

        $.ajax({
            url: "/report-builder/save-wizard-step-5",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(formData),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step6.reportId(json.model.reportId);
                self.parent.step6.relationships([]);
                $.each(json.model.relationships, function () {
                    var relationship = this;
                    self.parent.step6.relationships.push(new RelationshipModel(
                        relationship.tableName,
                        relationship.parentTable,
                        relationship.primaryKey,
                        relationship.foreignKey,
                        relationship.joinType,
                        relationship.availableColumns,
                        relationship.availableParentTables
                    ));
                });


                var relationCount = json.model.relationships.length;
                for (var i = 0; i < relationCount; i++) {
                    self.parent.step6.parentTable_onChange(i);
                }

                success = true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                success = false;
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.parent.translations.UpdateRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
            success = false;
        });

        return success;
    };
};

var WizardStep6Model = function (parent) {
    var self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.availableColumns = ko.observableArray([]);
    self.relationships = ko.observableArray([]);

    self.parentTable_onChange = function (index) {
        var relationship = self.relationships()[index];

        if (relationship.parentTable) {
            $.ajax({
                url: `/report-builder/get-columns/${self.reportId()}/${relationship.parentTable}`,
                type: "GET",
                dataType: "json",
                async: false
            })
            .done(function (json) {
                if (json.success) {
                    var select = $(`select[name="Relationships[${index}].PrimaryKey"]`);
                    select.find('option').remove();

                    $(json.columns).each(function (i, val) {
                        select.append($('<option>', {
                            value: val,
                            text: val,
                            selected: (relationship.primaryKeyBackup && val == relationship.primaryKeyBackup)
                        }));
                    });
                }
                else {
                    $.notify(json.message, "error");
                    console.log(json.message);
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                console.log(textStatus + ': ' + errorThrown);
            });
        }

        //var element = $(event.currentTarget).next();
    };

    self.save = function () {
        var success = false;

        var formData = form2js('wizard-form-6', '.', true);

        $.ajax({
            url: "/report-builder/save-wizard-step-6",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(formData),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step7.reportId(json.model.reportId);
                self.parent.step7.isDistinct(json.model.isDistinct);
                self.parent.step7.rowLimit(json.model.rowLimit);
                self.parent.step7.enumerationHandling(json.model.enumerationHandling);

                self.parent.step7.availableUsers([]);
                self.parent.step7.deniedUserIds([]);

                var hasAvailableUsers = json.model.availableUsers && json.model.availableUsers.length > 0;

                if (hasAvailableUsers > 0) {
                    $.each(json.model.availableUsers, function () {
                        self.parent.step7.availableUsers.push(this);
                    });
                }

                if (json.model.deniedUserIds && json.model.deniedUserIds.length > 0) {
                    $.each(json.model.deniedUserIds, function () {
                        self.parent.step7.deniedUserIds.push(this);
                    });
                }

                success = true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                success = false;
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.parent.translations.UpdateRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
            success = false;
        });

        return success;
    };
};

var WizardStep7Model = function (parent) {
    var self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.isDistinct = ko.observable(false);
    self.rowLimit = ko.observable(0);
    self.enumerationHandling = ko.observable(0);

    self.availableUsers = ko.observableArray([]);
    self.deniedUserIds = ko.observableArray([]);

    self.save = function () {
        var success = false;

        $.ajax({
            url: "/report-builder/save-wizard-step-7",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                ReportId: self.reportId(),
                IsDistinct: self.isDistinct(),
                RowLimit: self.rowLimit(),
                EnumerationHandling: self.enumerationHandling(),
                DeniedUserIds: self.deniedUserIds()
            }),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                success = true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                success = false;
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.parent.translations.UpdateRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
            success = false;
        });

        return success;
    };
};

var ReportModel = function (parent) {
    var self = this;
    self.parent = parent;

    self.step1 = new WizardStep1Model(self);
    self.step2 = new WizardStep2Model(self);
    self.step3 = new WizardStep3Model(self);
    self.step4 = new WizardStep4Model(self);
    self.step5 = new WizardStep5Model(self);
    self.step6 = new WizardStep6Model(self);
    self.step7 = new WizardStep7Model(self);
    
    self.create = function (groupId) {
        self.step1.id(0);
        self.step1.name(null);
        self.step1.groupId(groupId);
        self.step1.dataSourceId(null);
        self.step1.enabled(true);
        self.step1.emailEnabled(false);

        self.step7.isDistinct(false);
        self.step7.rowLimit(null);
        self.step7.enumerationHandling(0);

        $("#wizard").steps('reset');
        switchSection($("#wizard-section"));
    };

    self.edit = function (id) {
        $.ajax({
            url: `/report-builder/start-wizard/${id}`,
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.step1.id(json.model.id);
                self.step1.name(json.model.name);
                self.step1.groupId(json.model.groupId);
                self.step1.dataSourceId(json.model.dataSourceId);
                self.step1.enabled(json.model.enabled);
                self.step1.emailEnabled(json.model.emailEnabled);

                $("#wizard").steps('reset');
                switchSection($("#wizard-section"));
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.translations.getRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };
    self.remove = function (id) {
        if (confirm(self.parent.translations.deleteRecordConfirm)) {
            $.ajax({
                url: `${reportApiUrl}(${id})`,
                type: "DELETE",
                async: false
            })
            .done(function (json) {
                $('#Grid').data('kendoGrid').dataSource.read();
                $('#Grid').data('kendoGrid').refresh();

                $.notify(self.parent.translations.deleteRecordSuccess, "success");
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.deleteRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }
    };
    self.cancel = function () {
        switchSection($("#grid-section"));
    };
};

var ViewModel = function () {
    var self = this;

    self.dataSourceModel = new DataSourceModel(self);
    self.enumerationModel = new EnumerationModel(self);
    self.reportGroupModel = new ReportGroupModel(self);
    self.reportModel = new ReportModel(self);
    
    self.gridPageSize = 10;
    self.translations = false;

    self.init = function () {
        currentSection = $("#grid-section");

        // Load translations first, else will have errors
        $.ajax({
            url: "/report-builder/get-translations",
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            self.translations = json;
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            console.log(textStatus + ': ' + errorThrown);
        });

        self.gridPageSize = $("#GridPageSize").val();

        self.dataSourceModel.init();
        self.enumerationModel.init();
        self.reportGroupModel.init();
    };

    self.showDataSources = function () {
        switchSection($("#dataSources-grid-section"));
    };

    self.showEnumerations = function () {
        switchSection($("#enumerations-grid-section"));
    };
};

var viewModel;
$(document).ready(function () {
    // Initialize Wizard
    $("#wizard").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        stepsOrientation: "vertical",
        onStepChanging: function (event, currentIndex, newIndex) {
            if (currentIndex == newIndex) {
                // This happens when calling reset
                return false;
            }

            if (newIndex < currentIndex) {
                // Always allow previous step
                return true;
            };

            switch (currentIndex) {
                case 0: return viewModel.reportModel.step1.save();
                case 1: return viewModel.reportModel.step2.save();
                case 2: return viewModel.reportModel.step3.save();
                case 3: return viewModel.reportModel.step4.save();
                case 4: return viewModel.reportModel.step5.save();
                case 5: return viewModel.reportModel.step6.save();
                //case 6: return viewModel.reportModel.step7.save(); // This one is in 'onFinishing' function
                default: alert("Unknown step at index: " + currentIndex); return false;
            }
        },
        onFinishing: function (event, currentIndex) {
            //return viewModel.reportModel.step8.save();
            return viewModel.reportModel.step7.save();
        },
        onFinished: function (event, currentIndex) {
            $('#Grid').data('kendoGrid').dataSource.read();
            $('#Grid').data('kendoGrid').refresh();
            switchSection($("#grid-section"));
        }
    });
    
    viewModel = new ViewModel();
    ko.applyBindings(viewModel);
    viewModel.init();

    // Load Groups to dropdown
    viewModel.reportModel.step1.reloadGroups();
    viewModel.reportModel.step1.reloadDataSources();

    viewModel.reportModel.step1.groupId(($("#Report_GroupId option:first").val())); // Hack
    viewModel.reportModel.step1.dataSourceId(($("#Report_DataSourceId option:first").val()));

    $(document).on('click', 'td.clickable', function () {
        $(this).find('i').toggleClass('fa-plus fa-minus');
    });
});

$.fn.hasAttr = function (name) {
    var attr = this.attr(name);
    // For some browsers, `attr` is undefined; for others,
    // `attr` is false.  Check for both.
    return typeof attr !== typeof undefined && attr !== false;
};

ko.bindingHandlers.sortable.options.start = function (event, ui) {
    ui.item.toggleClass("highlight");
};

ko.bindingHandlers.sortable.options.stop = function (event, ui) {
    ui.item.toggleClass("highlight");
};

function lowerCase(str) {
    return str.toLowerCase();
}

function upperCase(str) {
    return str.toUpperCase();
}

function camelCase(str) {
    str = replaceAccents(str);
    str = removeNonWord(str)
        .replace(/\-/g, ' ') //convert all hyphens to spaces
        .replace(/\s[a-z]/g, upperCase) //convert first char of each word to UPPERCASE
        .replace(/\s+/g, '') //remove spaces
        .replace(/^[A-Z]/g, lowerCase); //convert first char to lowercase
    return str;
}

function unCamelCase(str) {
    str = str.replace(/([a-z\xE0-\xFF])([A-Z\xC0\xDF])/g, '$1 $2');
    str = str.toLowerCase(); //add space between camelCase text
    return str;
}

function pascalCase(str) {
    return camelCase(str).replace(/^[a-z]/, upperCase);
}

function titleCase(str) {
    str = str.toLowerCase().split(' ');

    let final = [];
    for (let word of str) {
        final.push(word.charAt(0).toUpperCase() + word.slice(1));
    }
    return final.join(' ')
}

function hasDuplicates(array) {
    return (new Set(array)).size !== array.length;
}