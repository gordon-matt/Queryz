class DataSourceModel {
    constructor(parent) {
        this.parent = parent;

        this.id = ko.observable(0);
        this.name = ko.observable(null);
        this.dataProvider = ko.observable(null);
        this.connectionDetails = ko.observable(null);
        this.validator = false;
    }

    init = () => {
        this.validator = $("#report-group-form-section-form").validate({
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
                template: '<div class="btn-group">' +
                    '<a data-bind="click: dataSourceModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + this.parent.translations.edit + '">' +
                    '<i class="fas fa-edit"></i></a>' +

                    '<a data-bind="click: dataSourceModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + this.parent.translations.delete + '">' +
                    '<i class="fas fa-xmark"></i></a>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 150
            }]
        });
    };

    create = () => {
        this.id(0);
        this.name(null);
        this.dataProvider(null);
        this.connectionDetails(null);

        this.cleanUpConnectionDetails();

        this.validator.resetForm();
        switchSection($("#dataSources-form-section"));
        $("#dataSources-form-section-legend").html(this.parent.translations.create);
    };

    edit = (id) => {
        fetch(`${dataSourceApiUrl}(${id})`)
            .then(response => response.json())
            .then(json => {
                this.id(json.Id);
                this.name(json.Name);

                fetch(`/report-builder/get-connection-details/${id}`)
                    .then(response => response.json())
                    .then(result => {
                        if (result.success) {
                            this.connectionDetails(result.connectionDetails);
                        }
                        else {
                            $.notify(result.message, "error");
                            console.log(result.message);
                        }

                        switch (json.DataProvider) {
                            case 'SqlServer': this.dataProvider(0); break;
                            case 'PostgreSql': this.dataProvider(1); break;
                            case 'MySql': this.dataProvider(2); break;
                            //default: $.notify(`Unknown data provider: '${json.DataProvider}'`, "error");
                        }

                        this.validator.resetForm();
                        switchSection($("#dataSources-form-section"));
                        $("#dataSources-form-section-legend").html(this.parent.translations.edit);
                    })
                    .catch(error => {
                        $.notify(this.parent.translations.getRecordError, "error");
                        console.error('Error: ', error);
                    });
            })
            .catch(error => {
                $.notify(this.parent.translations.getRecordError, "error");
                console.error('Error: ', error);
            });
    };

    remove = (id) => {
        if (confirm(this.parent.translations.deleteRecordConfirm)) {
            fetch(`${dataSourceApiUrl}(${id})`, { method: 'DELETE' })
                .then(response => {
                    if (response.ok) {
                        $('#DataSourceGrid').data('kendoGrid').dataSource.read();
                        $('#DataSourceGrid').data('kendoGrid').refresh();

                        $.notify(this.parent.translations.deleteRecordSuccess, "success");
                        this.parent.reportModel.step1.reloadDataSources();
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
        // ensure the function exists before calling it...
        if (typeof onBeforeSave == 'function') {
            onBeforeSave(this);
        }

        if (!$("#dataSources-form-section-form").valid()) {
            return false;
        }

        const record = {
            Id: this.id(),
            Name: this.name(),
            DataProvider: this.dataProvider(),
            ConnectionDetails: this.connectionDetails()
        };

        fetch(`${dataSourceApiUrl}/Default.Save`, {
            method: "POST",
            headers: {
                'Content-type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify(record)
        })
        .then(response => {
            if (response.ok) {
                $('#DataSourceGrid').data('kendoGrid').dataSource.read();
                $('#DataSourceGrid').data('kendoGrid').refresh();

                switchSection($("#dataSources-grid-section"));

                $.notify(this.parent.translations.insertRecordSuccess, "success");
                this.parent.reportModel.step1.reloadDataSources();
            } else {
                $.notify(this.parent.translations.insertRecordError, "error");
            }
        })
        .catch(error => {
            $.notify(this.parent.translations.insertRecordError, "error");
            console.error('Error: ', error);
        });
    };

    onDataProviderChanged = () => {
        const provider = this.dataProvider();

        if (provider == -1) {
            return false;
        }

        fetch(`/report-builder/get-connection-details-ui/${provider}`)
            .then(response => response.json())
            .then(json => {
                if (json.success) {
                    this.cleanUpConnectionDetails();

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
                        const data = ko.toJS(ko.mapping.fromJSON(this.connectionDetails()));
                        updateModel(this, data);
                        const elementToBind = $("#connection-details")[0];
                        ko.applyBindings(this.parent, elementToBind);
                    }
                }
                else {
                    $.notify(json.message, "error");
                    console.log(json.message);
                }
            })
            .catch(error => {
                $.notify(this.parent.translations.getRecordError, "error");
                console.error('Error: ', error);
            });
    };

    cleanUpConnectionDetails = () => {
        // Clean up from previously injected html/scripts
        if (typeof cleanUp === 'function') {
            cleanUp(this);
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

    cancel = () => {
        this.cleanUpConnectionDetails();
        switchSection($("#dataSources-grid-section"));
    };

    goBack = () => {
        this.cleanUpConnectionDetails();
        switchSection($("#grid-section"));
    };
}