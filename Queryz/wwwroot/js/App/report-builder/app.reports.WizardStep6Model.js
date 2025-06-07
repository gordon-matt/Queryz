class WizardStep6Model {
    constructor(parent) {
        this.parent = parent;

        this.reportId = ko.observable(0);
        this.availableColumns = ko.observableArray([]);
        this.relationships = ko.observableArray([]);
    }

    parentTable_onChange = (index) => {
        const relationship = this.relationships()[index];

        if (relationship.parentTable) {
            fetch(`/report-builder/get-columns/${this.reportId()}/${relationship.parentTable}`)
                .then(response => response.json())
                .then(json => {
                    if (json.success) {
                        const select = $(`select[name="Relationships[${index}].PrimaryKey"]`);
                        select.find('option').remove();

                        json.columns.forEach(item => {
                            select.append($('<option>', {
                                value: val,
                                text: val,
                                selected: (item.primaryKeyBackup && val == item.primaryKeyBackup)
                            }));
                        });
                    }
                    else {
                        $.notify(json.message, "error");
                        console.log(json.message);
                    }
                })
                .catch(error => {
                    console.error('Error: ', error);
                });
        }

        //const element = $(event.currentTarget).next();
    };

    save = () => {
        const formData = form2js('wizard-form-6', '.', true);

        return fetch("/report-builder/save-wizard-step-6", {
            method: "POST",
            headers: {
                'Content-type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify(formData)
        })
        .then(response => response.json())
        .then(json => {
            if (json.success) {
                this.parent.step7.reportId(json.model.reportId);
                this.parent.step7.isDistinct(json.model.isDistinct);
                this.parent.step7.rowLimit(json.model.rowLimit);
                this.parent.step7.enumerationHandling(json.model.enumerationHandling);

                this.parent.step7.availableUsers([]);
                this.parent.step7.deniedUserIds([]);

                const hasAvailableUsers = json.model.availableUsers && json.model.availableUsers.length > 0;

                if (hasAvailableUsers > 0) {
                    json.model.availableUsers.forEach(item => {
                        this.parent.step7.availableUsers.push(item);
                    });
                }

                if (json.model.deniedUserIds && json.model.deniedUserIds.length > 0) {
                    json.model.deniedUserIds.forEach(item => {
                        this.parent.step7.deniedUserIds.push(item);
                    });
                }

                return true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                return false;
            }
        })
        .catch(error => {
            $.notify(this.parent.parent.translations.updateRecordError, "error");
            console.error('Error: ', error);
            return false;
        });
    };
}