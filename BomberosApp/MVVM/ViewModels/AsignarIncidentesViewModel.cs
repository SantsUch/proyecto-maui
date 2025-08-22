using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class AsignarIncidentesViewModel : INotifyPropertyChanged
    {
        private readonly IncidentesRepository _incidentesRepository;
        private readonly UsuariosRepository _usuariosRepository;
        private readonly INavigation _navigation;
        private UsuarioModel _usuarioAdministrador;

        public ObservableCollection<IncidenteModel> IncidentesSinAsignar { get; set; }
        public ObservableCollection<UsuarioModel> FuncionariosDisponibles { get; set; }
        public ObservableCollection<string> CategoriasDisponibles { get; set; }
        public ObservableCollection<string> PrioridadesDisponibles { get; set; }
        public ObservableCollection<string> EstadosFiltro { get; set; }

        public IncidenteModel IncidenteSeleccionado { get; set; }
        public UsuarioModel FuncionarioSeleccionado { get; set; }
        public string CategoriaSeleccionada { get; set; }
        public string PrioridadSeleccionada { get; set; }
        public string EstadoFiltroSeleccionado { get; set; }
        public string ObservacionesAsignacion { get; set; }

        // Propiedades simples para el binding
        public bool MostrarPanelAsignacion => IncidenteSeleccionado != null;
        public string TituloIncidenteSeleccionado => IncidenteSeleccionado != null ? $"Incidente: {IncidenteSeleccionado.Titulo}" : "";

        // NUEVAS PROPIEDADES PARA MOSTRAR DETALLES
        public string DescripcionIncidenteSeleccionado => IncidenteSeleccionado?.Descripcion ?? "";
        public string UbicacionIncidenteSeleccionado => IncidenteSeleccionado?.Ubicacion ?? "";
        public string FechaIncidenteSeleccionado => IncidenteSeleccionado?.FechaReportado.ToString("dd/MM/yyyy HH:mm") ?? "";
        public bool MostrarImagenIncidente => !string.IsNullOrEmpty(IncidenteSeleccionado?.ImagenBase64);
        public string ImagenIncidenteBase64 => IncidenteSeleccionado?.ImagenBase64 ?? "";

        // Propiedades para mensajes informativos
        public string MensajeFuncionarios { get; set; }
        public bool MostrarMensajeFuncionarios => !string.IsNullOrEmpty(MensajeFuncionarios);

        // Propiedades para colores de prioridad
        public Color PrioridadColorFondo => PrioridadSeleccionada switch
        {
            "Crítica" => Color.FromArgb("#FFEBEE"), // Rojo claro
            "Alta" => Color.FromArgb("#FFF3E0"),     // Naranja claro
            "Media" => Color.FromArgb("#FFFDE7"),   // Amarillo claro
            "Baja" => Color.FromArgb("#E8F5E8"),    // Verde claro
            _ => Color.FromArgb("#F0F0F0")          // Gris por defecto
        };

        public Color PrioridadColorTexto => PrioridadSeleccionada switch
        {
            "Crítica" => Color.FromArgb("#C62828"), // Rojo oscuro
            "Alta" => Color.FromArgb("#E65100"),     // Naranja oscuro
            "Media" => Color.FromArgb("#F57F17"),   // Amarillo oscuro
            "Baja" => Color.FromArgb("#2E7D32"),    // Verde oscuro
            _ => Color.FromArgb("#666666")          // Gris oscuro por defecto
        };

        public ICommand ActualizarListaCommand { get; set; }
        public ICommand ActualizarFuncionariosCommand { get; set; }
        public ICommand SeleccionarIncidenteCommand { get; set; }
        public ICommand ConfirmarAsignacionCommand { get; set; }
        public ICommand CancelarAsignacionCommand { get; set; }
        public ICommand EliminarIncidenteCommand { get; set; }
        public ICommand CambiarEstadoCommand { get; set; } // RESTAURADO: Tanto admin como bomberos pueden cambiar estados

        public AsignarIncidentesViewModel(INavigation navigation)
        {
            _navigation = navigation;
            _incidentesRepository = new IncidentesRepository();
            _usuariosRepository = new UsuariosRepository();

            InitializeCollections();
            InitializeCommands();
            CargarDatos();
        }

        // Constructor que recibe el usuario administrador
        public AsignarIncidentesViewModel(INavigation navigation, UsuarioModel usuario) : this(navigation)
        {
            _usuarioAdministrador = usuario;
        }

        private void InitializeCollections()
        {
            IncidentesSinAsignar = new ObservableCollection<IncidenteModel>();
            FuncionariosDisponibles = new ObservableCollection<UsuarioModel>();

            CategoriasDisponibles = new ObservableCollection<string>
            {
                "Incendio Estructural",
                "Incendio Forestal",
                "Accidente de Tránsito",
                "Rescate en Altura",
                "Rescate Acuático",
                "Emergencia Médica",
                "Materiales Peligrosos",
                "Colapso Estructural",
                "Inundación",
                "Otro"
            };

            PrioridadesDisponibles = new ObservableCollection<string>
            {
                "Crítica",
                "Alta",
                "Media",
                "Baja"
            };

            EstadosFiltro = new ObservableCollection<string>
            {
                "Todos",
                "Sin Asignar",
                "Reportado",
                "Asignado",
                "En Proceso",
                "Resuelto"
            };

            EstadoFiltroSeleccionado = "Sin Asignar";
        }

        private void InitializeCommands()
        {
            ActualizarListaCommand = new Command(async () => await CargarIncidentes());
            ActualizarFuncionariosCommand = new Command(async () => await ActualizarFuncionarios());
            SeleccionarIncidenteCommand = new Command<IncidenteModel>(SeleccionarIncidente);
            ConfirmarAsignacionCommand = new Command(async () => await ConfirmarAsignacion());
            CancelarAsignacionCommand = new Command(CancelarAsignacion);
            EliminarIncidenteCommand = new Command<IncidenteModel>(async (incidente) => await EliminarIncidente(incidente));
            CambiarEstadoCommand = new Command<IncidenteModel>(async (incidente) => await CambiarEstado(incidente)); // RESTAURADO
        }

        // RESTAURADO: Método para cambiar estado - Tanto admin como bomberos pueden cambiar estados
        private async Task CambiarEstado(IncidenteModel incidente)
        {
            if (incidente == null) return;

            string nuevoEstado = await Application.Current.MainPage.DisplayActionSheet(
                "Cambiar estado del incidente",
                "Cancelar",
                null,
                IncidenteModel.Estados.Reportado,
                IncidenteModel.Estados.Asignado,
                IncidenteModel.Estados.EnProceso,
                IncidenteModel.Estados.Resuelto,
                IncidenteModel.Estados.Cancelado);

            if (!string.IsNullOrEmpty(nuevoEstado) && nuevoEstado != "Cancelar")
            {
                try
                {
                    await _incidentesRepository.CambiarEstadoIncidenteAsync(incidente.Id, nuevoEstado);

                    incidente.Estado = nuevoEstado;

                    await Application.Current.MainPage.DisplayAlert("Éxito", "El estado del incidente ha sido actualizado.", "OK");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cambiar el estado: {ex.Message}", "OK");
                }
            }
        }

        // REMOVIDO: Comentario que decía "Solo bomberos pueden cambiar estados"

        private async Task CargarDatos()
        {
            await CargarIncidentes();
            await CargarFuncionarios();
        }

        private async Task CargarIncidentes()
        {
            try
            {
                var todosIncidentes = await _incidentesRepository.ObtenerTodosAsync();
                var incidentesFiltrados = todosIncidentes
                    .Where(i => FiltrarPorEstado(i.Object))
                    .OrderByDescending(i => i.Object.FechaReportado)
                    .Select(i =>
                    {
                        var incidente = i.Object;
                        incidente.Key = i.Key; // Guardar la key para updates
                        return incidente;
                    })
                    .ToList();

                IncidentesSinAsignar.Clear();
                foreach (var incidente in incidentesFiltrados)
                {
                    IncidentesSinAsignar.Add(incidente);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar incidentes: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudieron cargar los incidentes", "OK");
            }
        }

        private bool FiltrarPorEstado(IncidenteModel incidente)
        {
            return EstadoFiltroSeleccionado switch
            {
                "Todos" => true,
                "Sin Asignar" => string.IsNullOrEmpty(incidente.Estado) || incidente.Estado == "Reportado",
                _ => incidente.Estado == EstadoFiltroSeleccionado
            };
        }

        // Método para eliminar un incidente
        private async Task EliminarIncidente(IncidenteModel incidente)
        {
            if (incidente == null) return;

            // 1. Mostramos una alerta de confirmación
            bool confirmacion = await Application.Current.MainPage.DisplayAlert(
                "Confirmar Eliminación",
                $"¿Estás seguro de que deseas eliminar el incidente '{incidente.Titulo}'? Esta acción no se puede deshacer.",
                "Sí, Eliminar",
                "Cancelar");

            if (confirmacion)
            {
                try
                {
                    // 2. Llamamos al repositorio para borrarlo de la base de datos
                    await _incidentesRepository.EliminarIncidenteAsync(incidente.Id);

                    // 3. Lo quitamos de la lista para que la UI se actualice al instante
                    IncidentesSinAsignar.Remove(incidente);

                    await Application.Current.MainPage.DisplayAlert("Éxito", "El incidente ha sido eliminado.", "OK");
                }
                catch (Exception ex)
                {
                    // Si algo sale mal, mostramos el error
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo eliminar el incidente: {ex.Message}", "OK");
                }
            }
        }

        private async Task CargarFuncionarios()
        {
            try
            {
                Console.WriteLine("Cargando funcionarios...");

                var usuarios = await _usuariosRepository.GetAllAsync();
                Console.WriteLine($"Total usuarios encontrados: {usuarios.Count}");

                var funcionarios = usuarios.Values
                    .Where(u => u.Rol == "Funcionario" || u.Rol == "Administrador")
                    .OrderBy(u => u.Nombre)
                    .ToList();

                Console.WriteLine($"Funcionarios filtrados: {funcionarios.Count}");

                FuncionariosDisponibles.Clear();
                foreach (var funcionario in funcionarios)
                {
                    Console.WriteLine($"Agregando funcionario: {funcionario.Nombre} - {funcionario.Rol}");

                    // Asegurar que tiene la Key de Firebase
                    var funcionarioConKey = usuarios.FirstOrDefault(u => u.Value.Id == funcionario.Id);
                    if (funcionarioConKey.Key != null)
                    {
                        funcionario.Key = funcionarioConKey.Key;
                    }

                    FuncionariosDisponibles.Add(funcionario);
                }

                Console.WriteLine($"Total funcionarios disponibles: {FuncionariosDisponibles.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar funcionarios: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudieron cargar los funcionarios disponibles", "OK");
            }
        }

        private async Task ActualizarFuncionarios()
        {
            MensajeFuncionarios = "Cargando funcionarios...";
            OnPropertyChanged(nameof(MensajeFuncionarios));
            OnPropertyChanged(nameof(MostrarMensajeFuncionarios));

            await CargarFuncionarios();

            if (FuncionariosDisponibles.Count == 0)
            {
                MensajeFuncionarios = "No hay funcionarios disponibles. Crear funcionarios en Gestión de Usuarios.";
            }
            else
            {
                MensajeFuncionarios = $"Funcionarios disponibles: {FuncionariosDisponibles.Count}";

                // Limpiar el mensaje después de 3 segundos
                Device.StartTimer(TimeSpan.FromSeconds(3), () =>
                {
                    MensajeFuncionarios = "";
                    OnPropertyChanged(nameof(MensajeFuncionarios));
                    OnPropertyChanged(nameof(MostrarMensajeFuncionarios));
                    return false;
                });
            }

            OnPropertyChanged(nameof(MensajeFuncionarios));
            OnPropertyChanged(nameof(MostrarMensajeFuncionarios));
        }

        private void SeleccionarIncidente(IncidenteModel incidente)
        {
            Console.WriteLine($"Seleccionando incidente: {incidente?.Titulo ?? "NULL"}");

            IncidenteSeleccionado = incidente;

            // Pre-cargar datos existentes si los hay
            CategoriaSeleccionada = incidente?.Categoria;
            PrioridadSeleccionada = incidente?.Prioridad;

            // Limpiar campos de asignación
            FuncionarioSeleccionado = null;
            ObservacionesAsignacion = string.Empty;

            // Notificar TODOS los cambios INCLUYENDO LAS NUEVAS PROPIEDADES DE DETALLES
            OnPropertyChanged(nameof(IncidenteSeleccionado));
            OnPropertyChanged(nameof(MostrarPanelAsignacion));
            OnPropertyChanged(nameof(TituloIncidenteSeleccionado));
            OnPropertyChanged(nameof(DescripcionIncidenteSeleccionado));
            OnPropertyChanged(nameof(UbicacionIncidenteSeleccionado));
            OnPropertyChanged(nameof(FechaIncidenteSeleccionado));
            OnPropertyChanged(nameof(MostrarImagenIncidente));
            OnPropertyChanged(nameof(ImagenIncidenteBase64));
            OnPropertyChanged(nameof(CategoriaSeleccionada));
            OnPropertyChanged(nameof(PrioridadSeleccionada));
            OnPropertyChanged(nameof(PrioridadColorFondo));
            OnPropertyChanged(nameof(PrioridadColorTexto));

            Console.WriteLine($"IncidenteSeleccionado asignado: {IncidenteSeleccionado?.Titulo ?? "NULL"}");
        }

        private async Task ConfirmarAsignacion()
        {
            Console.WriteLine("=== CONFIRMAR ASIGNACION ===");
            Console.WriteLine($"IncidenteSeleccionado: {IncidenteSeleccionado?.Titulo ?? "NULL"}");
            Console.WriteLine($"CategoriaSeleccionada: {CategoriaSeleccionada ?? "NULL"}");
            Console.WriteLine($"PrioridadSeleccionada: {PrioridadSeleccionada ?? "NULL"}");
            Console.WriteLine($"FuncionarioSeleccionado: {FuncionarioSeleccionado?.Nombre ?? "NULL"}");

            // Validación simple y directa
            if (IncidenteSeleccionado == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Debe seleccionar un incidente", "OK");
                return;
            }

            if (string.IsNullOrEmpty(CategoriaSeleccionada))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Debe seleccionar una categoría", "OK");
                return;
            }

            if (string.IsNullOrEmpty(PrioridadSeleccionada))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Debe seleccionar una prioridad", "OK");
                return;
            }

            if (FuncionarioSeleccionado == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Debe seleccionar un funcionario", "OK");
                return;
            }

            try
            {
                bool confirmar = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar Asignación",
                    $"¿Confirma la asignación del incidente '{IncidenteSeleccionado.Titulo}' " +
                    $"a {FuncionarioSeleccionado.Nombre}?",
                    "Sí", "No");

                if (!confirmar)
                    return;

                // Actualizar el incidente
                IncidenteSeleccionado.Categoria = CategoriaSeleccionada;
                IncidenteSeleccionado.Prioridad = PrioridadSeleccionada;
                IncidenteSeleccionado.FuncionarioAsignadoId = FuncionarioSeleccionado.Id;
                IncidenteSeleccionado.FuncionarioAsignadoNombre = FuncionarioSeleccionado.Nombre;
                IncidenteSeleccionado.Estado = "Asignado";
                IncidenteSeleccionado.FechaAsignacion = DateTime.Now;
                IncidenteSeleccionado.ObservacionesAsignacion = ObservacionesAsignacion;

                // Guardar en la base de datos
                await _incidentesRepository.ActualizarIncidenteAsync(IncidenteSeleccionado);

                await Application.Current.MainPage.DisplayAlert("Éxito",
                    "Incidente asignado correctamente", "OK");

                // Limpiar selección y recargar lista
                LimpiarSeleccion();
                await CargarIncidentes();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al asignar incidente: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudo asignar el incidente", "OK");
            }
        }

        private void CancelarAsignacion()
        {
            LimpiarSeleccion();
        }

        private void LimpiarSeleccion()
        {
            IncidenteSeleccionado = null;
            FuncionarioSeleccionado = null;
            CategoriaSeleccionada = null;
            PrioridadSeleccionada = null;
            ObservacionesAsignacion = string.Empty;

            // Notificar TODOS los cambios INCLUYENDO LAS NUEVAS PROPIEDADES DE DETALLES
            OnPropertyChanged(nameof(IncidenteSeleccionado));
            OnPropertyChanged(nameof(MostrarPanelAsignacion));
            OnPropertyChanged(nameof(TituloIncidenteSeleccionado));
            OnPropertyChanged(nameof(DescripcionIncidenteSeleccionado));
            OnPropertyChanged(nameof(UbicacionIncidenteSeleccionado));
            OnPropertyChanged(nameof(FechaIncidenteSeleccionado));
            OnPropertyChanged(nameof(MostrarImagenIncidente));
            OnPropertyChanged(nameof(ImagenIncidenteBase64));
            OnPropertyChanged(nameof(CategoriaSeleccionada));
            OnPropertyChanged(nameof(PrioridadSeleccionada));
            OnPropertyChanged(nameof(FuncionarioSeleccionado));
            OnPropertyChanged(nameof(PrioridadColorFondo));
            OnPropertyChanged(nameof(PrioridadColorTexto));
            OnPropertyChanged(nameof(MensajeFuncionarios));
            OnPropertyChanged(nameof(MostrarMensajeFuncionarios));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}