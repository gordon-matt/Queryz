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

        GridHelper.initKendoGrid(
            "EnumerationsGrid",
            enumerationApiUrl,
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
                template: '<div class="btn-group">' +
                    GridHelper.actionIconButton("enumerationModel.edit", 'fas fa-edit', this.parent.translations.edit, 'secondary', `#=Id#`) +
                    GridHelper.actionIconButton("enumerationModel.remove", 'fas fa-xmark', this.parent.translations.delete, 'danger', `#=Id#`) +
                    '</div>',
                attributes: { "class": "text-center" },
                filterable: false,
                width: 150
            }],
            this.parent.gridPageSize,
            { field: "Name", dir: "asc" });
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
                        GridHelper.refreshGrid('EnumerationsGrid');

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
                    GridHelper.refreshGrid('EnumerationsGrid');

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
                    GridHelper.refreshGrid('EnumerationsGrid');

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