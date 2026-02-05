export class SubstringFilter {

    // Returns all items where string field specified in 'fieldName' contains the substring specified in 'searchString' parameter.

    private static filter(items: any[], fieldName: string, searchString: string) {
        var filteredItems: any[] = [];

        _.forEach(items, item => {
            if(!searchString || (<string>item[fieldName]).toLocaleLowerCase().indexOf(searchString.toLocaleLowerCase()) !== -1)
                filteredItems.push(item);
        });

        return filteredItems;
    }

    public static create() {
        return SubstringFilter.filter;
    }
}
