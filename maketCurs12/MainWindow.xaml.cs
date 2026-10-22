using maketCurs12.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace maketCurs12
{
    public partial class MainWindow : Window
    {
        private Game _selectedGame;
        private string _selectedGamePhotoPath;
        private ObservableCollection<Mod> _currentMods;
        private CollectionViewSource _modsViewSource;

        public MainWindow()
        {
            InitializeComponent();
            _currentMods = new ObservableCollection<Mod>();

            // Настройка CollectionViewSource для фильтрации
            _modsViewSource = new CollectionViewSource();
            _modsViewSource.Source = _currentMods;
            _modsViewSource.Filter += ModsFilter;

            ModsItemsControl.ItemsSource = _modsViewSource.View;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (MyGamesItemsControl != null)
            {
                MyGamesItemsControl.ItemsSource = (DataContext as ViewModels.GamesViewModel)?.MyGames;
            }
        }

        // Фильтрация модов по категории
        private void ModsFilter(object sender, FilterEventArgs e)
        {
            var mod = e.Item as Mod;
            if (mod == null)
            {
                e.Accepted = false;
                return;
            }

            var comboBox = ModCategoryComboBox;
            if (comboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedCategory = selectedItem.Content?.ToString();

                if (selectedCategory == "Все моды")
                {
                    e.Accepted = true;
                }
                else
                {
                    // Проверяем соответствие категории или тегам
                    e.Accepted = mod.Category == selectedCategory || mod.Tags.Contains(selectedCategory);
                }
            }
            else
            {
                e.Accepted = true;
            }
        }

        // Удаление мода
        private void DeleteMod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button == null) return;

                Mod modToDelete = button.Tag as Mod;
                if (modToDelete == null) return;

                // Подтверждение удаления
                MessageBoxResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить мод \"{modToDelete.Name}\"?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Удаляем мод из коллекции
                    _currentMods.Remove(modToDelete);

                    // Обновляем представление
                    _modsViewSource.View.Refresh();

                    MessageBox.Show($"Мод \"{modToDelete.Name}\" успешно удален!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении мода: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void GameCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button == null) return;

                string gameName = button.Tag?.ToString();
                if (string.IsNullOrEmpty(gameName)) return;

                var viewModel = DataContext as ViewModels.GamesViewModel;
                if (viewModel != null)
                {
                    _selectedGame = viewModel.MyGames?.FirstOrDefault(g => g.Name == gameName) ??
                                   viewModel.NotAddedGames?.FirstOrDefault(g => g.Name == gameName);

                    if (_selectedGame != null)
                    {
                        _currentMods.Clear();
                        LoadGameInfo();
                        LoadModsForGame(gameName);

                        // Обновляем фильтр
                        _modsViewSource.View.Refresh();
                    }
                }

                MainPage.Visibility = Visibility.Collapsed;
                AddGamePage.Visibility = Visibility.Collapsed;
                ModsPage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddModButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Выберите архив с модом";
                openFileDialog.Filter = "Архивы|*.zip;*.rar;*.7z;*.tar;*.gz|Все файлы|*.*";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == true)
                {
                    string archivePath = openFileDialog.FileName;
                    string archiveName = Path.GetFileNameWithoutExtension(archivePath);

                    string iconPath = FindIconForMod(archivePath);
                    string description = FindDescriptionForMod(archivePath);

                    Mod newMod = new Mod
                    {
                        Name = archiveName,
                        Description = description,
                        IconPath = iconPath ?? GetIconPathForCategory(GetCategoryForMod(archiveName)),
                        Category = GetCategoryForMod(archiveName),
                        IsEnabled = false,
                        ArchivePath = archivePath
                    };

                    // Добавляем теги на основе названия
                    AddTagsToMod(newMod, archiveName);

                    _currentMods.Add(newMod);
                    _modsViewSource.View.Refresh();

                    MessageBox.Show($"Мод \"{archiveName}\" успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении мода: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTagsToMod(Mod mod, string modName)
        {
            modName = modName.ToLower();
            mod.Tags.Clear();

            if (modName.Contains("график") || modName.Contains("texture") || modName.Contains("hd") || modName.Contains("графика"))
                mod.Tags.Add("Графика");

            if (modName.Contains("gameplay") || modName.Contains("геймплей") || modName.Contains("механик"))
                mod.Tags.Add("Геймплей");

            if (modName.Contains("weapon") || modName.Contains("оружие") || modName.Contains("gun"))
                mod.Tags.Add("Оружие");

            if (modName.Contains("charact") || modName.Contains("персонаж") || modName.Contains("skin"))
                mod.Tags.Add("Персонажи");

            if (modName.Contains("звук") || modName.Contains("sound") || modName.Contains("audio"))
                mod.Tags.Add("Звук");

            if (modName.Contains("интерфейс") || modName.Contains("ui") || modName.Contains("hud"))
                mod.Tags.Add("Интерфейс");

            if (modName.Contains("квест") || modName.Contains("quest") || modName.Contains("миссия"))
                mod.Tags.Add("Квесты");

            // Если нет тегов, добавляем категорию как тег
            if (mod.Tags.Count == 0)
            {
                mod.Tags.Add(mod.Category);
            }
        }

        private string FindIconForMod(string archivePath)
        {
            string directory = Path.GetDirectoryName(archivePath);
            string fileName = Path.GetFileNameWithoutExtension(archivePath);

            string[] possibleIcons = new string[]
            {
                Path.Combine(directory, "icon.png"),
                Path.Combine(directory, "icon.jpg"),
                Path.Combine(directory, fileName + ".png"),
                Path.Combine(directory, fileName + ".jpg"),
                Path.Combine(directory, "preview.png"),
                Path.Combine(directory, "preview.jpg")
            };

            foreach (string iconPath in possibleIcons)
            {
                if (File.Exists(iconPath))
                {
                    return iconPath;
                }
            }

            return null;
        }

        private string FindDescriptionForMod(string archivePath)
        {
            string directory = Path.GetDirectoryName(archivePath);

            string[] possibleDescriptions = new string[]
            {
                Path.Combine(directory, "description.txt"),
                Path.Combine(directory, "readme.txt"),
                Path.Combine(directory, "info.txt")
            };

            foreach (string descPath in possibleDescriptions)
            {
                if (File.Exists(descPath))
                {
                    try
                    {
                        string content = File.ReadAllText(descPath);
                        if (content.Length > 200)
                        {
                            return content.Substring(0, 200) + "...";
                        }
                        return content;
                    }
                    catch { }
                }
            }

            return "Описание отсутствует";
        }

        private string GetCategoryForMod(string modName)
        {
            modName = modName.ToLower();

            if (modName.Contains("график") || modName.Contains("texture") || modName.Contains("hd") || modName.Contains("графика"))
                return "Графика";
            if (modName.Contains("gameplay") || modName.Contains("геймплей") || modName.Contains("механик"))
                return "Геймплей";
            if (modName.Contains("weapon") || modName.Contains("оружие") || modName.Contains("gun"))
                return "Оружие";
            if (modName.Contains("charact") || modName.Contains("персонаж") || modName.Contains("skin"))
                return "Персонажи";
            if (modName.Contains("звук") || modName.Contains("sound"))
                return "Звук";
            if (modName.Contains("интерфейс") || modName.Contains("ui"))
                return "Интерфейс";

            return "Другое";
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            GameNameInput.Text = "";
            GameGenreInput.Text = "";
            GameDescriptionInput.Text = "";
            GamePathInput.Text = "C:\\Games\\";
            GamePhotoImage.Source = null;
            GamePhotoImage.Visibility = Visibility.Collapsed;
            PhotoText.Visibility = Visibility.Visible;
            _selectedGamePhotoPath = null;

            MainPage.Visibility = Visibility.Collapsed;
            ModsPage.Visibility = Visibility.Collapsed;
            AddGamePage.Visibility = Visibility.Visible;
        }

        private void SelectGamePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Выберите изображение для игры";
                openFileDialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы|*.*";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == true)
                {
                    _selectedGamePhotoPath = openFileDialog.FileName;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedGamePhotoPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    GamePhotoImage.Source = bitmap;
                    GamePhotoImage.Visibility = Visibility.Visible;
                    PhotoText.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveNewGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GameNameInput.Text))
                {
                    MessageBox.Show("Введите название игры", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(GameGenreInput.Text))
                {
                    MessageBox.Show("Введите жанр игры", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Game newGame = new Game
                {
                    Name = GameNameInput.Text,
                    Genre = GameGenreInput.Text,
                    Description = GameDescriptionInput.Text,
                    ImagePath = _selectedGamePhotoPath,
                    IconPath = "C:\\Users\\voido\\Downloads\\icons\\default.png",
                    Status = "Установлена",
                    StatusColor = "#4CAF50",
                    IconColor = "#4CAF50"
                };

                var viewModel = DataContext as ViewModels.GamesViewModel;
                if (viewModel != null)
                {
                    viewModel.AddGame(newGame);
                    MyGamesItemsControl.ItemsSource = null;
                    MyGamesItemsControl.ItemsSource = viewModel.MyGames;
                }

                MainPage.Visibility = Visibility.Visible;
                AddGamePage.Visibility = Visibility.Collapsed;

                MessageBox.Show($"Игра \"{newGame.Name}\" успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelAddGame_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Visibility = Visibility.Visible;
            AddGamePage.Visibility = Visibility.Collapsed;
        }

        private void LoadGameInfo()
        {
            try
            {
                if (_selectedGame != null)
                {
                    if (SelectedGameNameText != null)
                        SelectedGameNameText.Text = _selectedGame.Name ?? "Неизвестная игра";

                    if (SelectedGameGenreText != null)
                        SelectedGameGenreText.Text = _selectedGame.Genre ?? "Неизвестный жанр";

                    if (SelectedGameDescriptionText != null)
                        SelectedGameDescriptionText.Text = _selectedGame.Description ?? "Описание отсутствует";

                    if (!string.IsNullOrEmpty(_selectedGame.ImagePath) && SelectedGameImage != null)
                    {
                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(_selectedGame.ImagePath, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            SelectedGameImage.Source = bitmap;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки информации об игре: {ex.Message}");
            }
        }

        private void LoadModsForGame(string gameName)
        {
            try
            {
                _currentMods.Clear();

                string path = "C:\\Users\\voido\\Desktop\\материал для фото\\кингдом.png";

                if (gameName.Contains("Cyberpunk"))
                {
                    AddModWithTags("Graphics Overhaul", "Улучшает графику и текстуры", "Графика", path, true, new[] { "Графика", "HD", "Текстуры" });
                    AddModWithTags("New Weapons Pack", "Добавляет новое оружие", "Оружие", path, false, new[] { "Оружие", "Оружейный" });
                    AddModWithTags("Cyber Engine Tweak", "Оптимизация и фиксы", "Геймплей", path, true, new[] { "Геймплей", "Оптимизация" });
                    AddModWithTags("Character Customization", "Новые возможности кастомизации", "Персонажи", path, false, new[] { "Персонажи", "Кастомизация" });
                    AddModWithTags("Weather Controller", "Управление погодой", "Графика", path, false, new[] { "Графика", "Погода" });
                    AddModWithTags("Realistic Driving", "Реалистичное вождение", "Геймплей", path, true, new[] { "Геймплей", "Физика" });
                    AddModWithTags("Sound Overhaul", "Новые звуки и музыка", "Звук", path, false, new[] { "Звук", "Аудио" });
                }
                else if (gameName.Contains("Witcher"))
                {
                    AddModWithTags("HD Reworked", "Текстуры высокого разрешения", "Графика", path, false, new[] { "Графика", "HD" });
                    AddModWithTags("New Quests", "Дополнительные квесты", "Квесты", path, true, new[] { "Квесты", "Сюжет" });
                    AddModWithTags("Combat Improved", "Улучшенная боевая система", "Геймплей", path, true, new[] { "Геймплей", "Бой" });
                    AddModWithTags("New Characters", "Новые персонажи", "Персонажи", path, false, new[] { "Персонажи", "NPC" });
                    AddModWithTags("Better Lighting", "Улучшенное освещение", "Графика", path, true, new[] { "Графика", "Освещение" });
                }
                else if (gameName.Contains("Dota"))
                {
                    AddModWithTags("Custom Skins", "Новые скины для героев", "Персонажи", path, false, new[] { "Персонажи", "Скины" });
                    AddModWithTags("UI Enhancer", "Улучшенный интерфейс", "Интерфейс", path, true, new[] { "Интерфейс", "UI" });
                    AddModWithTags("Particle Effects", "Новые эффекты", "Графика", path, true, new[] { "Графика", "Эффекты" });
                    AddModWithTags("Voice Pack", "Новые голоса", "Звук", path, false, new[] { "Звук", "Голоса" });
                }
                else if (gameName.Contains("CS") || gameName.Contains("Counter"))
                {
                    AddModWithTags("Weapon Skins", "Новые скины оружия", "Оружие", path, false, new[] { "Оружие", "Скины" });
                    AddModWithTags("Crosshair Customizer", "Настройка прицела", "Интерфейс", path, true, new[] { "Интерфейс", "Настройки" });
                    AddModWithTags("Map Textures", "Новые текстуры карт", "Графика", path, false, new[] { "Графика", "Карты" });
                    AddModWithTags("Sound Pack", "Новые звуки", "Звук", path, true, new[] { "Звук", "Аудио" });
                }
                else
                {
                    AddModWithTags("Graphics Pack", "Улучшение графики", "Графика", path, false, new[] { "Графика" });
                    AddModWithTags("Gameplay Tweaks", "Изменения геймплея", "Геймплей", path, false, new[] { "Геймплей" });
                    AddModWithTags("Weapons Pack", "Новое оружие", "Оружие", path, true, new[] { "Оружие" });
                    AddModWithTags("Characters Pack", "Новые персонажи", "Персонажи", path, false, new[] { "Персонажи" });
                }

                _modsViewSource.View.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки модов: {ex.Message}");
            }
        }

        private void AddModWithTags(string name, string description, string category, string path, bool isEnabled, string[] tags)
        {
            Mod mod = new Mod
            {
                Name = name,
                Description = description,
                Category = category,
                IsEnabled = isEnabled,
                IconPath = path ?? GetIconPathForCategory(category)
            };

            foreach (var tag in tags)
            {
                mod.Tags.Add(tag);
            }

            _currentMods.Add(mod);
        }

        private string GetIconPathForCategory(string category)
        {
            switch (category)
            {
                case "Графика": return "C:\\Users\\voido\\Downloads\\icons\\graphics.png";
                case "Геймплей": return "C:\\Users\\voido\\Downloads\\icons\\gameplay.png";
                case "Оружие": return "C:\\Users\\voido\\Downloads\\icons\\weapon.png";
                case "Персонажи": return "C:\\Users\\voido\\Downloads\\icons\\character.png";
                case "Звук": return "C:\\Users\\voido\\Downloads\\icons\\sound.png";
                case "Интерфейс": return "C:\\Users\\voido\\Downloads\\icons\\ui.png";
                case "Квесты": return "C:\\Users\\voido\\Downloads\\icons\\quest.png";
                default: return "C:\\Users\\voido\\Downloads\\icons\\default.png";
            }
        }

        private void ModToggle_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox toggle = sender as CheckBox;
                if (toggle == null) return;

                string modName = toggle.Tag?.ToString() ?? "Неизвестный мод";
                bool isEnabled = toggle.IsChecked == true;
                System.Diagnostics.Debug.WriteLine($"Мод {modName} теперь {(isEnabled ? "включен" : "выключен")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка переключения мода: {ex.Message}");
            }
        }

        private void ModCategory_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Обновляем фильтрацию при изменении выбранной категории
                _modsViewSource?.View?.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка фильтрации категорий: {ex.Message}");
            }
        }

        private void BackToGames_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainPage != null && ModsPage != null)
                {
                    MainPage.Visibility = Visibility.Visible;
                    ModsPage.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при возврате: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}