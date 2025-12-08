function deleteMovieById(movieId)
{
    var context = getContext();
    var collection = context.getCollection();
    var response = context.getResponse();

    var query = 'SELECT * FROM Movies m WHERE m.id ="' + movieId + '"';
    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        query,
        function(error, documents)
        {
           if(error) 
              throw error;
           
            if(documents.length > 0)
            {
                var doc = documents[0];
                collection.deleteDocument(doc._self, function(error)
                {
                    if(error)
                        throw error;
                    response.setBody('Movie with id ' + movieId + ' deleted successfully.');
                });
            }
            else
            response.setBody("Movie not found.")
        });
        if(!isAccepted) throw new Error("Query not accepted.");
}