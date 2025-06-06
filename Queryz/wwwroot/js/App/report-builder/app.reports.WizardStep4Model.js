var WizardStep4Model = function (parent) {
    const self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.query = ko.observable(null);

    self.id = -1;

    self.init = function (model) {
        self.reportId(model.reportId);
        self.query(model.query);
        
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
                console.log(`Error when parsing query: ${model.query}.`)
                console.log(`The error message is as follows: ${x.message}.`)
            }
        }
    };
    
    self.save = function () {
        //const result = $('#query-builder').queryBuilder('getSQL', false);
        const result = JSON.stringify($('#query-builder').queryBuilder('getRules'));

        let success = false;
        
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
                    const sorting = this;
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