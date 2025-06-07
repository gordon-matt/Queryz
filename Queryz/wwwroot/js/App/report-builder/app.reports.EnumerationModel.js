class EnumerationModel {
    constructor(parent) {
        this.parent = parent;

        this.id = ko.observable(0);
        this.name = ko.observable(null);
        this.isBitFlags = ko.observable(false);
        this.values = ko.observableArray([]);

        this.newValueId = 0; // A temporary value, auto-incremented.. used for automatically setting the next value. Can still be changed by user, if needed.

        this.validator = false;
    }

    init = () => {
        this.validator = $("#enumerations-form-section-form").validate({
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
                    '<a data-bind="click: enumerationModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + this.parent.translations.edit + '">' +
                    '<i class="fas fa-edit"></i></a>' +

                    '<a data-bind="click: enumerationModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + this.parent.translations.delete + '">' +
                    '<i class="fas fa-xmark"></i></a>' +
                    '</div>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 150
            }]
        });
    };

    create = () => {
        this.id(0);
        this.name(null);
        this.isBitFlags(false);
        this.values([]);
        this.newValueId = 0;

        this.validator.resetForm();
        switchSection($("#enumerations-form-section"));
        $("#enumerations-form-section-legend").html(this.parent.translations.create);
    };

    edit = (id) => {
        this.values([]);
        this.newValueId = 0;

        fetch(`${enumerationApiUrl}(${id})`)
            .then(response => response.json())
            .then(json => {
                this.id(json.Id);
                this.name(json.Name);
                this.isBitFlags(json.IsBitFlags);

                if (json.Values) {
                    const data = ko.toJS(ko.mapping.fromJSON(json.Values));
                    data.forEach(item => {
                        this.values.push(item);
                        this.newValueId = item.id + 1;
                    });
                }

                this.validator.resetForm();
                switchSection($("#enumerations-form-section"));
                $("#enumerations-form-section-legend").html(this.parent.translations.edit);
            })
            .catch(error => {
                $.notify(this.parent.translations.getRecordError, "error");
                console.error('Error: ', error);
            });
    };

    remove = (id) => {
        if (confirm(this.parent.translations.deleteRecordConfirm)) {
            fetch(`${enumerationApiUrl}(${id})`, { method: 'DELETE' })
                .then(response => {
                    if (response.ok) {
                        $('#EnumerationsGrid').data('kendoGrid').dataSource.read();
                        $('#EnumerationsGrid').data('kendoGrid').refresh();

                        $.notify(this.parent.translations.deleteRecordSuccess, "success");
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

        if (!$("#enumerations-form-section-form").valid()) {
            return false;
        }

        const valuesJson = ko.mapping.toJSON(this.values());

        const record = {
            Id: this.id(),
            Name: this.name(),
            IsBitFlags: this.isBitFlags(),
            Values: valuesJson
        };

        if (isNew) {
            fetch(enumerationApiUrl, {
                method: "POST",
                headers: {
                    'Content-type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify(record)
            })
            .then(response => {
                if (response.ok) {
                    $('#EnumerationsGrid').data('kendoGrid').dataSource.read();
                    $('#EnumerationsGrid').data('kendoGrid').refresh();

                    switchSection($("#enumerations-grid-section"));

                    $.notify(this.parent.translations.insertRecordSuccess, "success");
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
            fetch(`${enumerationApiUrl}(${this.id()})`, {
                method: "PUT",
                headers: {
                    'Content-type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify(record)
            })
            .then(response => {
                if (response.ok) {
                    $('#EnumerationsGrid').data('kendoGrid').dataSource.read();
                    $('#EnumerationsGrid').data('kendoGrid').refresh();

                    switchSection($("#enumerations-grid-section"));

                    $.notify(this.parent.translations.updateRecordSuccess, "success");
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

    cancel = () => {
        switchSection($("#enumerations-grid-section"));
    };

    goBack = () => {
        switchSection($("#grid-section"));
    };

    addEnumerationValue = () => {
        this.values.push({ id: this.newValueId, name: null });
        this.newValueId++;
    };

    removeEnumerationValue = () => {
        this.values.remove(this);
    };
}