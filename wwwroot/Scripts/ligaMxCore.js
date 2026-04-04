$( document ).ready(function() {
  $('#btnUpdateScores').click(function(event) {
        event.preventDefault(); // Detiene la navegación
        updateScores();
        // Aquí puedes agregar tu función de jQuery personalizada
    });


  function updateScores() {
        var scores = [];
        $('.match_score').each(function() {
            var match = $(this);
            var scoreData = {
                JornadaPartidoId: parseInt(match.data('id')),
                JornadaId: parseInt(match.find('input[name="JornadaId"]').val()),
                EstadioId: parseInt(match.find('input[name="EstadioId"]').val()),
                PartidoId: parseInt(match.find('input[name="PartidoId"]').val()),
                GolLocal: parseInt(match.find('input[name="golLocal"]').val()),
                GolVisita: parseInt(match.find('input[name="golVisita"]').val()),
                EstatusPartidoId: parseInt(match.find('input[name="EstatusPartidoId"]').val()),
                TipoResultadoId: parseInt(match.find('input[name="TipoResultadoId"]').val())
            };
            scores.push(scoreData);
        });
        
        $.ajax({
            // Usa la ruta absoluta con un "/" al inicio o el Helper de MVC
            url: '/JornadaPartido/UpdateScores', 
            type: 'POST',
            // IMPORTANTE: No envuelvas 'scores' en otro objeto
            data: JSON.stringify(scores), 
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (response) {
                alert("Procesados: " + response.message);
            },
            error: function (xhr) {
                console.error("Error: ", xhr.responseText);
            }
        });
    };

    function updateScores_OnSuccess(response) {
        alert(response.message);
    }

    function updateScores_OnError(response) {
        alert(response.message);
    }



});