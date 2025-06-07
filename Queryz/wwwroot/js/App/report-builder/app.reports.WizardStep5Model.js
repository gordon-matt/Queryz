class WizardStep5Model {
    constructor(parent) {
        this.parent = parent;

        this.reportId = ko.observable(0);
        this.availableColumns = ko.observableArray([]);
        this.sortings = ko.observableArray([]);

        this.validator = false;
    }

    init = () => {
        this.validator = $("#wizard-form-5").validate();
    }

    addSorting = () => {
        this.sortings.push({
            columnName: null,
            sortDirection: 0
        });
    };

    removeSorting = () => {
        this.sortings.remove(this);
    };

    save = () => {
        if (!$("#wizard-form-5").valid()) {
            return false;
        }

        const formData = form2js('wizard-form-5', '.', true);

        return fetch("/report-builder/save-wizard-step-5", {
            method: "POST",
            headers: {
                'Content-type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify(formData)
        })
        .then(response => response.json())
        .then(json => {
            if (json.success) {
                this.parent.step6.reportId(json.model.reportId);
                this.parent.step6.relationships([]);

                json.model.relationships.forEach(item => {
                    this.parent.step6.relationships.push(new RelationshipModel(
                        item.tableName,
                        item.parentTable,
                        item.primaryKey,
                        item.foreignKey,
                        item.joinType,
                        item.availableColumns,
                        item.availableParentTables
                    ));
                });

                const relationCount = json.model.relationships.length;
                for (let i = 0; i < relationCount; i++) {
                    this.parent.step6.parentTable_onChange(i);
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