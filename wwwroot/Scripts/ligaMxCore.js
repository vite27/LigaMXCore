$( document ).ready(function() {

  // Inicializa un combo hijo en cascada que depende de un combo padre (ej.
  // País -> Estado en Municipio, Temporada -> Jornada en JornadaPartido).
  // Los ids de los combos son exclusivos de cada vista, así que un mismo
  // handler delegado no afecta a otras páginas aunque ligaMxCore.js sea
  // compartido por todo el sitio.
  //
  // Evita la condición de carrera de pedir el combo hijo dos veces seguido
  // muy rápido (el usuario cambia de padre antes de que la primera llamada
  // AJAX responda): cada petición lleva un número de secuencia local y solo
  // la respuesta de la ÚLTIMA petición disparada se aplica al combo; una
  // respuesta tardía de una petición anterior se descarta aunque llegue
  // después (fue el bug encontrado al verificar la cascada de Municipio).
  function initCascada(selectPadreId, selectHijoId, urlBase, textoTodos) {
      var ultimaPeticion = 0;

      $('#' + selectPadreId).on('change', function () {
          var padreId = $(this).val() || 0;
          var selectHijo = $('#' + selectHijoId);
          var valorActual = selectHijo.val();
          var peticionActual = ++ultimaPeticion;

          $.ajax({
              url: urlBase + padreId,
              type: 'GET',
              dataType: 'json',
              success: function (items) {
                  if (peticionActual !== ultimaPeticion) {
                      return; // respuesta obsoleta de un cambio anterior, se descarta
                  }

                  selectHijo.empty();
                  selectHijo.append($('<option></option>').val('').text(textoTodos));

                  var sigueExistiendo = false;
                  $.each(items, function (i, item) {
                      if (String(item.id) === String(valorActual)) {
                          sigueExistiendo = true;
                      }
                      selectHijo.append($('<option></option>').val(item.id).text(item.nombre));
                  });

                  selectHijo.val(sigueExistiendo ? valorActual : '');
              },
              error: function (xhr) {
                  console.error('Error al cargar opciones de ' + selectHijoId + ': ', xhr.responseText);
              }
          });
      });
  }

  initCascada('muniPaisId', 'muniEstadoId', '/Municipio/EstadosPorPais/', 'Todos los estados');
  initCascada('jpTemporadaId', 'jpJornadaId', '/JornadaPartido/JornadasPorTemporada/', 'Selecciona una jornada');

  // Botón de estatus de cada fila (Iniciar / Finalizar): pide el cambio al servidor y, si
  // se acepta, reemplaza SOLO esa fila por la que devuelve el servidor (así no se pierden los
  // marcadores sin guardar de las demás filas). Si la fila tiene textboxes, manda su marcador
  // para que se guarde junto con el cambio de estatus. Mientras responde, la fila queda
  // bloqueada para evitar dobles clics que avancen dos estados.
  $(document).on('click', '.btn-estatus', function () {
      var boton = $(this);
      var fila = boton.closest('tr.match_score');
      if (fila.data('ocupada')) {
          return;
      }

      var golLocal = fila.find('input[name="golLocal"]');
      var golVisita = fila.find('input[name="golVisita"]');
      var datos = {
          JornadaPartidoId: parseInt(fila.data('id')),
          Accion: boton.data('accion'),
          GolLocal: null,
          GolVisita: null
      };
      if (golLocal.length && golVisita.length) {
          if (!/^\d+$/.test($.trim(golLocal.val())) || !/^\d+$/.test($.trim(golVisita.val()))) {
              alert('Los goles deben ser números enteros mayores o iguales a 0.');
              return;
          }
          datos.GolLocal = parseInt(golLocal.val());
          datos.GolVisita = parseInt(golVisita.val());
      }

      fila.data('ocupada', true);
      boton.prop('disabled', true);

      $.ajax({
          url: '/JornadaPartido/CambiarEstatus',
          type: 'POST',
          data: JSON.stringify(datos),
          contentType: 'application/json; charset=utf-8',
          dataType: 'html',
          headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').first().val() },
          success: function (html) {
              fila.replaceWith($.trim(html));
          },
          error: function (xhr) {
              fila.data('ocupada', false);
              boton.prop('disabled', false);
              alert(xhr.responseText || 'No se pudo cambiar el estatus del partido.');
          }
      });
  });

  $('#btnUpdateScores').click(function(event) {
        event.preventDefault(); // Detiene la navegación
        updateScores();
        // Aquí puedes agregar tu función de jQuery personalizada
    });


  function updateScores() {
        var scores = [];
        // Solo las filas con textboxes de marcador (en jornadas de solo lectura la
        // vista renderiza spans, no hay nada que enviar).
        $('.match_score').has('input[name="golLocal"]').each(function() {
            var match = $(this);
            var scoreData = {
                JornadaPartidoId: parseInt(match.data('id')),
                JornadaId: parseInt(match.find('input[name="JornadaId"]').val()),
                EstadioId: parseInt(match.find('input[name="EstadioId"]').val()),
                PartidoId: parseInt(match.find('input[name="PartidoId"]').val()),
                GolLocal: parseInt(match.find('input[name="golLocal"]').val()),
                GolVisita: parseInt(match.find('input[name="golVisita"]').val())
                // TipoResultadoId y EstatusPartidoId ya no se envían: el servidor calcula el
                // resultado a partir de los goles y el estatus solo cambia con CambiarEstatus.
            };
            scores.push(scoreData);
        });
        
        if (scores.length === 0) {
            return;
        }

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