class RelationshipModel {
    constructor(tableName, parentTable, primaryKey, foreignKey, joinType, availableColumns, availableParentTables) {
        this.tableName = tableName;
        this.parentTable = parentTable;
        this.primaryKey = primaryKey; // Why does this keep disappearing??? Is it a reseverd word?
        this.foreignKey = foreignKey;
        this.joinType = joinType;
        this.availableColumns = availableColumns;
        this.availableParentTables = availableParentTables;

        this.primaryKeyBackup = primaryKey; // Have extra primary Key field because other one keeps disappearing.
    }
}