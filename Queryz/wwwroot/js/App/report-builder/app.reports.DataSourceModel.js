var DataSourceModel = function (parent) {
    const self = this;
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
                template:
                    '<div class="btn-group">' +
                    '<a data-bind="click: dataSourceModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + self.parent.translations.edit + '">' +
                    '<i class="fas fa-edit"></i></a>' +

                    '<a data-bind="click: dataSourceModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + self.parent.translations.delete + '">' +
                    '<i class="fas fa-xmark"></i></a>',
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

        const record = {
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
        const provider = self.dataProvider();

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

                const result = $(json.html);

                // Add new HTML
                const content = $(result.filter('#connection-details-content')[0]);
                const details = $('<div>').append(content.clone()).html();
                $("#connection-details").html(details);

                // Add new Scripts
                const scripts = result.filter('script');

                $.each(scripts, function () {
                    const script = $(this);
                    script.attr("data-settings-script", "true");
                    script.appendTo('body');
                });

                // Update Bindings
                // Ensure the function exists before calling it...
                if (typeof updateModel == 'function') {
                    const data = ko.toJS(ko.mapping.fromJSON(self.connectionDetails()));
                    updateModel(self, data);
                    const elementToBind = $("#connection-details")[0];
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
        let oldScripts = $('script[data-settings-script="true"]');

        if (oldScripts.length > 0) {
            $.each(oldScripts, function () {
                $(this).remove();
            });
        }

        const elementToBind = $("#connection-details")[0];
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