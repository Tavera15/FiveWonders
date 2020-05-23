# Five Wonders Party Website Using ASP.NET

## Progress
* Setup Project and initial commit
* Product Manager Controller can now Create, Edit, and Delete records, and store them in memory cache.
* Created Category Repository and Controller. Manager can now Create, Edit, and Delete records and store them in memory cache.
* Refactored repository code files into one file and interface. Also implemented a Product Management View Model.
* Updated Product Manager View Model to include list of Subcategories.
* Created a Size Chart Manager Controller. Can create a chart and store its values in a multi-array.
* Remodeled Size Chart Model and Manager Controller to only include an image url and a list of sizes to display.
* Updated Product Manager to display all available size charts and select one.
* Created a Product Order View Model to transfer each product's data to a view.
* Setup database to insert, edit, and delete data for all current models.
* Products model is updated to include an array of images associated with each product.
* Navigation bar is now dynamic to include custom categories. A route was created to return products from a certain category.
* Created a route to get a list of products that has a matching category, subcategory, or both.