class SelectedColumnModel {
    constructor(columnName, type, alias, isLiteral, displayColumn, enumerationId, transformFunction, format, isHidden) {
        this.columnName = columnName;
        this.type = ko.observable(type);
        this.alias = ko.observable(alias);
        this.isLiteral = isLiteral;
        this.displayColumn = displayColumn;
        this.enumerationId = enumerationId;
        this.transformFunction = transformFunction;
        this.format = format;
        this.isHidden = isHidden;
    }
}