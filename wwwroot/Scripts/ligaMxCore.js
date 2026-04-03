$( document ).ready(function() {
  $('#btnUpdateScores').click(function(event) {
        event.preventDefault(); // Detiene la navegación
        loadScores();
        // Aquí puedes agregar tu función de jQuery personalizada
    });


  function loadScores() {
        var scores = [];
        $('.match_score').each(function() {
            var match = $(this);
            var scoreData = {
                JornadaPartidoId: match.data('id'),
                JornadaId: match.find('input[name="JornadaId"]').val(),
                EstadioId: match.find('input[name="EstadioId"]').val(),
                PartidoId: match.find('input[name="PartidoId"]').val(),
                EstatusPartidoId: match.find('input[name="EstatusPartidoId"]').val(),
                GolLocal: match.find('input[name="golLocal"]').val(),
                GolVisita: match.find('input[name="golVisita"]').val()
            };
            scores.push(scoreData);
        });
        var jsonScores = JSON.stringify(scores);
        console.log(jsonScores); // Para depurar, muestra el JSON en consola
        //return jsonScores; // Devuelve el JSON para usarlo después (ej. en AJAX)
    };

      //$('.match_score').each(function() {
      //    var fila = $(this);
      //    var item = {
      //        Id: fila.data('id') // Obtenemos el ID del atributo data
      //    };

      //    alert('ID del partido: ' + item.Id); // Muestra el ID del partido en una alerta

          // Recorremos los inputs dentro de esta fila para llenar el objeto
          //fila.find('.dato-input').each(function() {
          //    var input = $(this);
          //    item[input.attr('name')] = input.val(); 
      //  });

          //listaProductos.push(item);
  //};

});