class WizardStep1Model {
    constructor(parent) {
        this.parent = parent;

        this.id = ko.observable(0);
        this.name = ko.observable(null);
        this.groupId = ko.observable(0);
        this.dataSourceId = ko.observable(0);
        this.enabled = ko.observable(false);
        this.emailEnabled = ko.observable(false);

        this.validator = false;

        this.availableGroups = ko.observableArray([]);
        this.availableDataSources = ko.observableArray([]);
    }

    init = () => {
        this.validator = $("#wizard-form-1").validate();
    }

    save = () => {
        if (!$("#wizard-form-1").valid()) {
            return false;
        }

        return fetch("/report-builder/save-wizard-step-1", {
            method: "POST",
            headers: {
                'Content-type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify({
                Id: this.id(),
                Name: this.name(),
                GroupId: this.groupId(),
                DataSourceId: this.dataSourceId(),
                Enabled: this.enabled(),
                EmailEnabled: this.emailEnabled()
            })
        })
        .then(response => response.json())
        .then(json => {
            if (json.success) {
                this.parent.step2.reportId(json.model.reportId);
                this.parent.step2.availableTables([]);
                this.parent.step2.selectedTables([]);

                json.model.availableTables.forEach(item => {
                    this.parent.step2.availableTables.push(item);
                });

                json.model.selectedTables.forEach(item => {
                    this.parent.step2.selectedTables.push(item);
                });

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

    reloadGroups = () => {
        // We use timeout to prevent error when both the grid and this try to call the same url at the same time. It causes
        //  a NotSupportedException on the db context
        setTimeout(() => {
            fetch(reportGroupApiUrl)
                .then(response => response.json())
                .then(json => {
                    this.availableGroups([]);
                    json.value.forEach(item => {
                        this.availableGroups.push({ id: item.Id, name: item.Name });
                    });
                })
                .catch(error => {
                    $.notify("Could not reload groups", "error");
                    console.error('Error: ', error);
                });
        }, 1000);
    };

    reloadDataSources = () => {
        // We use timeout to prevent error when both the grid and this try to call the same url at the same time. It causes
        //  a NotSupportedException on the db context
        setTimeout(() => {
            fetch(`${dataSourceApiUrl}?$orderby=Name`)
                .then(response => response.json())
                .then(json => {
                    this.availableDataSources([]);
                    json.value.forEach(item => {
                        this.availableDataSources.push({ id: item.Id, name: item.Name });
                    });
                })
                .catch(error => {
                    $.notify("Could not reload data sources", "error");
                    console.error('Error: ', error);
                });
        }, 1000);
    };
}