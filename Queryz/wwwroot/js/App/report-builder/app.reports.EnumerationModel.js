var EnumerationModel = function (parent) {
    const self = this;
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
                        '<a data-bind="click: enumerationModel.edit.bind($data,#=Id#)" class="btn btn-secondary" title="' + self.parent.translations.edit + '">' +
                        '<i class="fas fa-edit"></i></a>' +

                        '<a data-bind="click: enumerationModel.remove.bind($data,#=Id#)" class="btn btn-danger" title="' + self.parent.translations.delete + '">' +
                        '<i class="fas fa-xmark"></i></a>' +
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
                const data = ko.toJS(ko.mapping.fromJSON(json.Values));
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
        const isNew = (self.id() == 0);

        if (!$("#enumerations-form-section-form").valid()) {
            return false;
        }

        const valuesJson = ko.mapping.toJSON(self.values());

        const record = {
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