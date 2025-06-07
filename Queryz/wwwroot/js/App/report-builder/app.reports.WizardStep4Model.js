class WizardStep4Model {
    constructor(parent) {
        this.parent = parent;

        this.reportId = ko.observable(0);
        this.query = ko.observable(null);

        this.id = -1;
    }

    init = (model) => {
        this.reportId(model.reportId);
        this.query(model.query);

        $('#query-builder').queryBuilder('destroy');

        let queryBuilderConfig = model.jqQueryBuilderConfig;
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
                console.log(`Error when parsing query: ${model.query}.`);
                console.log(`The error message is as follows: ${x.message}.`);
            }
        }
    };

    save = () => {
        //const result = $('#query-builder').queryBuilder('getSQL', false);
        const result = JSON.stringify($('#query-builder').queryBuilder('getRules'));

        return fetch("/report-builder/save-wizard-step-4", {
            method: "POST",
            headers: {
                'Content-type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify({
                ReportId: this.reportId(),
                //Query: result.sql
                Query: result
            })
        })
        .then(response => response.json())
        .then(json => {
            if (json.success) {
                this.parent.step5.reportId(json.model.reportId);
                this.parent.step5.availableColumns([]);
                this.parent.step5.sortings([]);

                json.model.availableColumns.forEach(item => {
                    this.parent.step5.availableColumns.push(item);
                });

                json.model.sortings.forEach(item => {
                    this.parent.step5.sortings.push({
                        columnName: item.columnName,
                        sortDirection: item.sortDirection
                    });
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
}