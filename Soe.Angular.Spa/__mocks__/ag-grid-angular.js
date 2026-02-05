// Mock for ag-grid-angular to avoid ES module issues in Jest
module.exports = {
  AgGridAngular: {
    __esModule: true,
    default: {
      selector: 'ag-grid-angular',
      template: '<div></div>',
      standalone: true,
    },
  },
};
