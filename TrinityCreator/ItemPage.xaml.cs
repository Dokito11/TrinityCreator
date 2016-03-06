﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;

namespace TrinityCreator
{
    /// <summary>
    /// Interaction logic for ArmorWeaponPage.xaml
    /// </summary>
    public partial class ItemPage : UserControl
    {
        public ItemPage(Item _item, ItemPreview _preview)
        {
            InitializeComponent();
            item = _item;

            // load preview
            preview = _preview;
            previewBox.Content = preview;

            // Set quality
            itemQualityCb.ItemsSource = ItemQuality.GetQualityList();
            itemQualityCb.SelectedIndex = 0;

            // Set class & subclass
            itemClassCb.ItemsSource = ItemClass.GetClassList();
            itemClassCb.SelectedIndex = 0;

            ShowCorrectClassBox();
            armorBox.Visibility = Visibility.Collapsed;
            entryIdTxt.Text = Properties.Settings.Default.nextid_item.ToString();

            // Set item bounds
            itemBoundsCb.ItemsSource = ItemBonding.GetItemBondingList();

            // Load flags groupbox
            item.Flags = BitmaskStackPanel.GetItemFlags();
            flagsBitMaskGroupBox.Content = item.Flags;
            flagsBitMaskGroupBox.Visibility = Visibility.Collapsed; // by default

            // Load FlagsExtra
            item.FlagsExtra = BitmaskStackPanel.GetItemFlagsExtra();
            flagsExtraBitMaskGroupBox.Content = item.FlagsExtra;
            flagsExtraBitMaskGroupBox.Visibility = Visibility.Collapsed; // by default

            // load allowedclass groupbox
            item.AllowedClass = BitmaskStackPanel.GetClassFlags();
            limitClassBitMaskGroupBox.Content = item.AllowedClass;
            limitClassBitMaskGroupBox.Visibility = Visibility.Collapsed;
            preview.PrepareClassLimitations(item.AllowedClass);

            // load allowedrace groupbox
            item.AllowedRace = BitmaskStackPanel.GetRaceFlags();
            limitRaceBitMaskGroupBox.Content = item.AllowedRace;
            limitRaceBitMaskGroupBox.Visibility = Visibility.Collapsed;
            preview.PrepareRaceLimitations(item.AllowedRace);

            // Set weapon groupbox
            item.DamageInfo = new Damage();
            damageTypeCb.ItemsSource = DamageType.GetDamageTypes();
            damageTypeCb.SelectedIndex = 0;

            // Set resistance groupbox
            item.Resistances = new DynamicDataControl(
                DamageType.GetDamageTypes(magicOnly: true), 6, unique: true);
            addResistanceGroupBox.Visibility = Visibility.Collapsed;
            addResistanceGroupBox.Content = item.Resistances;
            
            // Set gemSockets groupbox
            item.GemSockets = new DynamicDataControl(
                Socket.GetSocketList(), 3, unique: false, header1:"Socket Type", header2:"Amount", defaultValue: "0");
            gemsGroupBox.Visibility = Visibility.Collapsed;
            preview.gemsPanel.Visibility = Visibility.Collapsed;
            gemSocketsSp.Content = item.GemSockets;
            socketBonusCb.ItemsSource = SocketBonus.GetBonusList();
            socketBonusCb.SelectedIndex = 0;
            item.GemSockets.Changed += GemDataChangedHander;
        }

        Item item;
        ItemPreview preview;


        #region Changed event handlers
        private void itemNameTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            preview.itemNameLbl.Content = itemNameTxt.Text;
        }
        private void itemQuoteTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (itemQuoteTxt.Text == "")
                    preview.itemQuoteLbl.Visibility = Visibility.Collapsed;
                else
                {
                    preview.itemQuoteLbl.Visibility = Visibility.Visible;
                    preview.itemQuoteLbl.Content = '"' + itemQuoteTxt.Text + '"';
                }
            }
            catch { /* Exception on initial load */ }
        }
        private void itemQualityCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemQuality q = (ItemQuality)itemQualityCb.SelectedValue;
            preview.itemNameLbl.Foreground = new SolidColorBrush(q.QualityColor);

            // set/unset account bound flags
            try
            {
                var bmcbr1 = from cb in item.Flags.Children.OfType<BitmaskCheckBox>()
                            where (uint)cb.Tag == 134217728
                            select cb;
                var bmcbr2 = from cb in item.FlagsExtra.Children.OfType<BitmaskCheckBox>()
                             where (uint)cb.Tag == 131072
                             select cb;
                if (q.Id == 7)
                {
                    bmcbr1.FirstOrDefault().IsChecked = true;
                    bmcbr2.FirstOrDefault().IsChecked = true;
                }
                else
                {
                    bmcbr1.FirstOrDefault().IsChecked = false;
                    bmcbr2.FirstOrDefault().IsChecked = false;
                }
            }
            catch
            { /* Exception on initial load */ }
        }
        
        private void itemClassCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemClass c = (ItemClass)itemClassCb.SelectedValue;
            itemSubClassCb.ItemsSource = ItemSubClass.GetSubclassList(c);
            itemSubClassCb.SelectedIndex = 0;
            ShowCorrectClassBox();
        }

        private void itemSubClassCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemSubClass sc = (ItemSubClass)itemSubClassCb.SelectedValue;
            if (sc == null)
                return;

            // Load equip
            inventoryTypeCb.ItemsSource = sc.LockedInventoryType;
            inventoryTypeCb.SelectedIndex = 0;

            // Load correct box
            if (sc.PreviewNoteLeft == "")
                preview.subclassLeftNoteLbl.Visibility = Visibility.Collapsed;
            else
            {
                preview.subclassLeftNoteLbl.Content = sc.PreviewNoteLeft;
                preview.subclassRightNoteLbl.Visibility = Visibility.Visible;
            }

            if (sc.PreviewNoteRight == "")
                preview.subclassRightNoteLbl.Visibility = Visibility.Collapsed;
            else
            {
                preview.subclassRightNoteLbl.Content = sc.PreviewNoteRight;
                preview.subclassRightNoteLbl.Visibility = Visibility.Visible;
            }
        }

        private void itemBoundsCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemBonding b = (ItemBonding)itemBoundsCb.SelectedValue;
            preview.itemBoundsLbl.Content = b.Description;

            // Don't display when no bounds
            if (b.Id == 0)
                preview.itemBoundsLbl.Visibility = Visibility.Collapsed;
            else
                preview.itemBoundsLbl.Visibility = Visibility.Visible;
        }

        private void inventoryTypeCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemInventoryType it = (ItemInventoryType)inventoryTypeCb.SelectedValue;
            if (it == null)
                return;

            preview.subclassLeftNoteLbl.Content = it.Description;
        }

        private void changeFlagsCb_Checked(object sender, RoutedEventArgs e)
        {
            flagsBitMaskGroupBox.Visibility = Visibility.Visible;
            flagsExtraBitMaskGroupBox.Visibility = Visibility.Visible;
        }
        private void changeFlagsCb_Unchecked(object sender, RoutedEventArgs e)
        {
            flagsBitMaskGroupBox.Visibility = Visibility.Collapsed;
            flagsExtraBitMaskGroupBox.Visibility = Visibility.Collapsed;
        }

        private void limitClassCb_Checked(object sender, RoutedEventArgs e)
        {
            limitClassBitMaskGroupBox.Visibility = Visibility.Visible;
            preview.itemClassRequirementsLbl.Visibility = Visibility.Visible;
        }
        private void limitClassCb_Unchecked(object sender, RoutedEventArgs e)
        {
            limitClassBitMaskGroupBox.Visibility = Visibility.Collapsed;
            if (preview.itemClassRequirementsLbl.Content.ToString().Contains(": All"))
                preview.itemClassRequirementsLbl.Visibility = Visibility.Collapsed;
        }
        private void limitRaceCb_Checked(object sender, RoutedEventArgs e)
        {
            limitRaceBitMaskGroupBox.Visibility = Visibility.Visible;
            preview.itemRaceRequirementsLbl.Visibility = Visibility.Visible;
        }
        private void limitRaceCb_Unchecked(object sender, RoutedEventArgs e)
        {
            limitRaceBitMaskGroupBox.Visibility = Visibility.Collapsed;
            if (preview.itemRaceRequirementsLbl.Content.ToString().Contains(": All"))
                preview.itemRaceRequirementsLbl.Visibility = Visibility.Collapsed;
        }
        private void addResistancesCb_Checked(object sender, RoutedEventArgs e)
        {
            addResistanceGroupBox.Visibility = Visibility.Visible;
        }
        private void addResistancesCb_Unchecked(object sender, RoutedEventArgs e)
        {
            addResistanceGroupBox.Visibility = Visibility.Collapsed;
        }


        private void buyPriceGTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                preview.buyGoldLbl.Content = buyPriceGTxt.Text;
                preview.buyDockPanel.Visibility = Visibility.Visible;
            } catch { /* Exception on initial load */ }
        }
        private void buyPriceSTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                preview.buySilverLbl.Content = buyPriceSTxt.Text;
                preview.buyDockPanel.Visibility = Visibility.Visible;
            } catch { /* Exception on initial load */ }
        }
        private void buyPriceCTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                preview.buyCopperLbl.Content = buyPriceCTxt.Text;
                preview.buyDockPanel.Visibility = Visibility.Visible;
            } catch { /* Exception on initial load */ }
        }
        private void sellPriceGTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                preview.sellGoldLbl.Content = sellPriceGTxt.Text;
                preview.sellDockPanel.Visibility = Visibility.Visible;
            } catch { /* Exception on initial load */ }
        }
        private void sellPriceSTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                preview.sellSilverLbl.Content = sellPriceSTxt.Text;
                preview.sellDockPanel.Visibility = Visibility.Visible;
            } catch { /* Exception on initial load */ }
        }
        private void sellPriceCTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                preview.sellCopperLbl.Content = sellPriceCTxt.Text;
                preview.sellDockPanel.Visibility = Visibility.Visible;
            } catch { /* Exception on initial load */ }
        }

        private void damageTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                item.DamageInfo.MinDamage = int.Parse(damageMinTxt.Text);
                item.DamageInfo.MaxDamage = int.Parse(damageMaxTxt.Text);
                preview.weaponMinMaxDmgLbl.Content = string.Format("({0} - {1} Damage)", damageMinTxt.Text, damageMaxTxt.Text);
                preview.weaponDpsLbl.Content = item.DamageInfo.GetDpsString();
            }
            catch { /* Exception on initial load or invalid value*/ }
        }
        private void weaponSpeedTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                item.DamageInfo.Speed = int.Parse(weaponSpeedTxt.Text);
                double viewSpeed = (double)item.DamageInfo.Speed / 1000;
                preview.weaponSpeedLbl.Content = string.Format("Speed {0}", viewSpeed.ToString("0.00"));
                preview.weaponDpsLbl.Content = item.DamageInfo.GetDpsString();
            }
            catch
            {
                preview.weaponSpeedLbl.Content = "Speed INVALID";
            }
        }
        private void damageTypeCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            item.DamageInfo.Type = (DamageType)damageTypeCb.SelectedValue;
            preview.weaponDpsLbl.Content = item.DamageInfo.GetDpsString();
        }

        private void addGemSocketsCb_Checked(object sender, RoutedEventArgs e)
        {
            gemsGroupBox.Visibility = Visibility.Visible;
            preview.gemsPanel.Visibility = Visibility.Visible;
        }
        private void addGemSocketsCb_Unchecked(object sender, RoutedEventArgs e)
        {
            gemsGroupBox.Visibility = Visibility.Collapsed;
            preview.gemsPanel.Visibility = Visibility.Collapsed;
        }

        private void durabilityTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (durabilityTxt.Text != "" && durabilityTxt.Text != "0")
                {
                    preview.itemDurabilityLbl.Content = string.Format("(Durability {0} / {0})", durabilityTxt.Text);
                    preview.itemDurabilityLbl.Visibility = Visibility.Visible;
                }
                else
                    preview.itemDurabilityLbl.Visibility = Visibility.Collapsed;
            }
            catch { /* Exception on initial load or invalid value*/ }
        }

        private void itemPlayerLevelTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (itemPlayerLevelTxt.Text != "0" && itemPlayerLevelTxt.Text != "")
            {
                preview.itemLevelRequiredLbl.Content = "Requires Level " + itemPlayerLevelTxt.Text;
                preview.itemLevelRequiredLbl.Visibility = Visibility.Visible;
            }
            else
                preview.itemLevelRequiredLbl.Visibility = Visibility.Collapsed;

        }

        private void GemDataChangedHander(object sender, EventArgs e)
        {
            preview.gemsPanel.Children.Clear();
            try
            {
                foreach (var line in item.GemSockets.GetUserInput())
                {
                    int count = int.Parse(line.Value);
                    Socket sock = (Socket)line.Key;
                    for (int i = 0; i < count; i++)
                    {
                        DockPanel dp = new DockPanel();
                        dp.Margin = new Thickness(5, 0, 0, 0);

                        Image img = new Image();
                        img.Source = sock.SocketImage;
                        img.Width = 15;
                        img.Height = 15;
                        dp.Children.Add(img);

                        Label lab = new Label();
                        lab.Content = sock.Description + " Socket";
                        lab.Foreground = Brushes.Gray;
                        lab.Margin = new Thickness(0, -5, 0, 0);
                        dp.Children.Add(lab);

                        preview.gemsPanel.Children.Add(dp);
                    }
                }
            }
            catch
            {
                preview.gemsPanel.Children.Clear();
            }
        }

        private void socketBonusCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SocketBonus sb = (SocketBonus)socketBonusCb.SelectedValue;
            if (sb.Id != 0)
            {
                preview.socketBonusLbl.Visibility = Visibility.Visible;
                preview.socketBonusLbl.Content = "Socket Bonus: " + sb.Description;
            }
            else
                preview.socketBonusLbl.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Click event handlers
        private void exportSqlBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                try
                {
                    item.EntryId = int.Parse(entryIdTxt.Text);
                }
                catch
                {
                    throw new Exception("Invalid entry ID");
                }
                item.Quote = itemQuoteTxt.Text;
                item.Class = (ItemClass)itemClassCb.SelectedValue;
                item.ItemSubClass = (ItemSubClass)itemSubClassCb.SelectedValue;
                item.Name = itemNameTxt.Text;
                try
                {
                    item.DisplayId = int.Parse(displayIdTxt.Text);
                }
                catch
                {
                    throw new Exception("Invalid display ID");
                }
                item.Quality = (ItemQuality)itemQualityCb.SelectedValue;
                item.Binds = (ItemBonding)itemBoundsCb.SelectedValue;
                try
                {
                    item.MinLevel = int.Parse(itemPlayerLevelTxt.Text);
                }
                catch
                {
                    item.MinLevel = 0;
                }

                try
                {
                    item.MaxAllowed = int.Parse(itemMaxCountTxt.Text);
                }
                catch
                {
                    item.MaxAllowed = 0;
                }
                //item.AllowedClass; Already set in constructor
                //item.AllowedRace; Already set in constructor
                item.ValueBuy = new Currency(buyPriceGTxt.Text, buyPriceSTxt.Text, buyPriceCTxt.Text).Amount;
                item.ValueSell = new Currency(sellPriceGTxt.Text, sellPriceSTxt.Text, sellPriceCTxt.Text).Amount;
                item.InventoryType = (ItemInventoryType)inventoryTypeCb.SelectedValue;
                // Material set in ItemSubClass
                // sheath set in InventoryType
                // Flags set in constructor
                // FlagsExtra set in constructor
                try
                {
                    item.BuyCount = int.Parse(buyCountTxt.Text);
                }
                catch
                {
                    item.BuyCount = 1;
                }

                try
                {
                    item.Stackable = int.Parse(itemStackCountTxt.Text);
                }
                catch
                {
                    item.Stackable = 1;
                }

                try
                {
                    item.ContainerSlots = int.Parse(containerSlotsTxt.Text);
                }
                catch
                {
                    item.ContainerSlots = 0;
                }
                // dmg_min, dmg_max, dmg_type, delay is changed in item with valid changedevents
                // resistances set in constructor

                // set ammo_type if needed
                ItemSubClass isc = (ItemSubClass)itemSubClassCb.SelectedValue;
                if (isc.Description == "Bow")
                    item.AmmoType = 2;
                else if (isc.Description == "Gun")
                    item.AmmoType = 3;
                else item.AmmoType = 0;

                // set durability
                try
                {
                    item.Durability = int.Parse(durabilityTxt.Text);
                }
                catch
                {
                    item.Durability = 0;
                }
                // sockets set in constructor
                item.SocketBonus = (SocketBonus)socketBonusCb.SelectedValue;


                string query = item.GenerateSqlQuery();
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".sql";
                sfd.FileName = "Item " + item.EntryId;
                sfd.Filter = "SQL File (.sql)|*.sql";
                if (sfd.ShowDialog() == true)
                    System.IO.File.WriteAllText(sfd.FileName, query);
                
                // Increase next item's entry id
                Properties.Settings.Default.nextid_item = int.Parse(entryIdTxt.Text) + 1;
                Properties.Settings.Default.Save();

                MessageBox.Show("Your item has been saved.", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to generate query.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void newItemBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to discard this item and clear the form?", "Discard item", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
                ClearForm();
        }
        #endregion


        /// <summary>
        /// Shows & hides UI groupboxes for certain item classes
        /// </summary>
        private void ShowCorrectClassBox()
        {
            // Hide everything
            weaponBox.Visibility = Visibility.Collapsed;
            armorBox.Visibility = Visibility.Collapsed;
            equipmentBox.Visibility = Visibility.Collapsed;
            containerBox.Visibility = Visibility.Collapsed;
            vendorBox.Visibility = Visibility.Collapsed;
            addResistancesCb.Visibility = Visibility.Collapsed;
            preview.weaponPanel.Visibility = Visibility.Collapsed;
            preview.armorPanel.Visibility = Visibility.Collapsed;
            addGemSocketsCb.Visibility = Visibility.Collapsed;
            statsBox.Visibility = Visibility.Collapsed;
            preview.otherStatsLbl.Visibility = Visibility.Collapsed;
            preview.itemDurabilityLbl.Visibility = Visibility.Collapsed;

            // Show selected
            ItemClass selectedClass = (ItemClass)itemClassCb.SelectedValue;
            switch (selectedClass.Id)
            {
                case 0: // Consumable

                    break;
                case 1: // Container
                    containerBox.Visibility = Visibility.Visible;
                    vendorBox.Visibility = Visibility.Visible;
                    break;
                case 2: // Weapon
                    weaponBox.Visibility = Visibility.Visible;
                    equipmentBox.Visibility = Visibility.Visible;
                    vendorBox.Visibility = Visibility.Visible;
                    addResistancesCb.Visibility = Visibility.Visible;
                    preview.weaponPanel.Visibility = Visibility.Visible;
                    addGemSocketsCb.Visibility = Visibility.Visible;
                    statsBox.Visibility = Visibility.Visible;
                    preview.otherStatsLbl.Visibility = Visibility.Visible;
                    preview.itemDurabilityLbl.Visibility = Visibility.Visible;
                    break;
                case 3: // Gems
                    vendorBox.Visibility = Visibility.Visible;
                    break;
                case 4: // Armor
                    armorBox.Visibility = Visibility.Visible;
                    equipmentBox.Visibility = Visibility.Visible;
                    vendorBox.Visibility = Visibility.Visible;
                    addResistancesCb.Visibility = Visibility;
                    preview.armorPanel.Visibility = Visibility.Visible;
                    addGemSocketsCb.Visibility = Visibility.Visible;
                    statsBox.Visibility = Visibility.Visible;
                    preview.otherStatsLbl.Visibility = Visibility.Visible;
                    preview.itemDurabilityLbl.Visibility = Visibility.Visible;
                    break;
                case 5: // Reagent
                    vendorBox.Visibility = Visibility.Visible;
                    break;
                case 6: // Projectile
                    vendorBox.Visibility = Visibility.Visible;
                    break;
                case 7: // Trade goods

                    break;
                case 9: // Recipe

                    break;
                case 11: // Quiver

                    break;
                case 12: // Quest

                    break;
                case 13: // Key

                    break;
                case 15: // Miscellaneous

                    break;
                case 16: // Glyph

                    break;
            }
        }

        private void ClearForm()
        {
            entryIdTxt.Text = Properties.Settings.Default.nextid_item.ToString();
            MessageBox.Show("Not implemented yet"); // Probably just load new ItemPage, don't clear all the fields manually :P
        }       
    }
}
