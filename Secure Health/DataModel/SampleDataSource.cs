using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace Secure_Health.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : Secure_Health.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");

            var group1 = new SampleDataGroup("Group-1",
                    "Skin Problems",
                    "Home Remedies for Dry and Oily Skin",
                    "Assets/skinproblem.jpg",
                    "Problem related to oily and dry skin are unpleasant. Here we'll offer some home remedies that will get you closer to the smooth skin you desire.");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "1: Wash Your Face",
                    "Oily Skin",
                    "Assets/washface.jpg",
                    "Lemon juice is great for oily skin. Wash your face regularly with fresh water 2-3 times a day.",
                    "In most cases, oily skin can be treated at home. However, you'll want to call a doctor if you develop acne that doesn't respond to home remedies or notice any sudden and/or unusual change in your skin (if it goes from dry to oily seemingly overnight but it isn't time for your period, for example). Otherwise, first try keeping skin squeaky clean. As anyone with oily skin knows, the oilier the skin, the dirtier the skin looks and feels. To help combat this feeling, it's important to keep the skin clean by washing it at least twice a day. Some doctors recommend detergent-type soap. You might even try adding a drop or two of dishwashing detergent to your regular soap; the extra kick will act as a solvent for the oil. However, other dermatologists say detergent soaps are just too harsh even for oily facial skin, recommending instead twice-daily cleansing with a glycerin soap. If you try a detergent soap and find it too irritating for your skin, try the glycerin variety, generally available over the counter in the skin-care aisle of most drugstores. \nAbsorb Extra Oil : Try aloe vera. Apply aloe vera gel (available in many drugstores as well as health-food stores) to your face to absorb oil and clear out pores. Dab the gel onto your face two to three times a day (especially after washing), then let it dry. The gel will feel more refreshing if it's cool, so keep it in the refrigerator. Wipe with astringents. Wiping the oily parts of the face with rubbing alcohol or a combination of alcohol and acetone (a mixture found in products such as Seba-Nil Liquid Cleanser) can help degrease your skin just as well as more expensive, perfumey astringents. Many drugstores even sell premoistened, individually wrapped alcohol wipes that you can keep in your purse for quick touchups throughout the day. Carry tissues. Even if you don't have an astringent with you, paper facial tissues can help soak up excess oils in a pinch. You can also purchase special oil-absorbing tissues at the cosmetics counter that are very effective in removing excess oil between cleansings. Chill out with cold water rinses. If you don't want to apply chemicals to your skin, simply splashing your face with cold water and blotting it dry a couple of times a day can help remove some excess oil",
                    group1));
           
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "2: Avoid Oils",
                    "Oily Skin",
                    "Assets/avoidoil.jpg",
                    "Pull your hair back. It's best to keep hair away from the face if you are having issues with your skin. Often oily hair and oily skin go together.",
                    "Pull your hair back. It's best to keep hair away from the face if you are having issues with your skin. Often oily hair and oily skin go together. Don't touch. Keep your hands off your face during the day. Hands deliver excess oil and dirt. Use water-based cosmetics. Better yet, learn to live without makeup -- or at least without foundation -- since it will simply add to and trap the oil against your skin and set the stage for blemishes. If you feel you must use makeup, choose water-based products over oil-based types, and opt for spot concealers rather than coating your entire face. In general, stick with powder or gel blushers, and avoid cream foundations. Still, sometimes a person needs a little extra help. Go to the next page to learn natural home remedies that you can find in your very own kitchen. \nMake a Scrub or Masque : Giving your face a very light scrub can remove excess surface oil. Try this almond honey scrub: Mix a small amount of almond meal (ground almonds) with honey. Then gently massage (don't scrub) the paste onto your skin with a hot washcloth. Rinse thoroughly. You can also make a scrub from oatmeal mixed with aloe vera. Rub gently onto the skin, leave on for 15 minutes, then wash off thoroughly. If you suffer from acne on your face, however, you should probably skip the scrub, since it can aggravate your already-irritated skin. Masques applied to the face can also reduce oiliness. Clay masques are available, or you can mix Fuller's Earth (available at pharmacies) with a little water to make a paste. Apply to the face and leave on for about 20 minutes before thoroughly rinsing off.",
                    group1));
           
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "3: Use Cornstarch and Salt",
                    "Oily Skin",
                    "Assets/consalt.jpg",
                    "A salt spray can do wonders for oily skin...",
                    "Cornstarch. Cornstarch dries up oily patches. Mix 1 to 3 tablespoons cornstarch with enough warm water to make a paste. Rub on your face, let dry, and then shower or rinse off with lukewarm water in the sink. Try this once a day for best results. \nSalt. This gift from the sea is nature's best desiccant. Place tepid water into a small spray bottle and add 1 teaspoon salt. Close your eyes, and pretend you're at the seashore. Then squirt some of this salt spray on your face once during the day. Blot dry..",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "4: Exfoliate",
                    "Oily Skin",
                    "Assets/exfoliate.jpg",
                    "Baking soda has many health and beauty uses, including removing oil and blackheads..",
                    "Be abrasive, but in a mild way. Liquid soap users can add 1/2 teaspoon baking soda into the mixture. Rub gently onto oily areas such as the nose and chin. This gentle abrasive works well in getting rid of blackheads as well as oil. Rinse with cool water. Another good way to exfoliate the skin is with white or apple cider vinegar. Apply using a cotton ball before bedtime. Leave it on for five to ten minutes and then rinse with cool water. You'll need to use this remedy for three weeks to see improvements. If your skin is super-sensitive, dilute the vinegar with four parts water. For a summertime treat, chill the vinegar or freeze it into ice cubes and apply as a cooling facial.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "5: Make an Apple Facial",
                    "Oily Skin",
                    "Assets/apples.jpg",
                    "Apples are something of a wonder fruit, not only rich in fiber and vitamin C, but also useful in a facial..",
                    "If you're willing to do some creative cooking, your effort will be rewarded with this homemade, oil-ridding facial. Mix 1/2 cup mashed apple, 1/2 cup cooked oatmeal, 1 slightly beaten egg white, and 1 tablespoon lemon juice into a smooth paste. Apply to your face for 15 minutes, then rinse with cool water. \nRefresh Skin With Fruit \n Citrus fruits and some vegetables not only refresh the skin but also help reduce oils. Mix equal parts lemon juice and water, pat on face, and let dry. Rinse first with warm water followed by cool water for a refreshing treat. You can also try mixing 1/2 teaspoon lime juice with an equal amount of cucumber juice. Apply to skin a few minutes before showering.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-6",
                   "6: Try Egg Yok",
                   "Oily Skin",
                   "Assets/eggyolk.jpg",
                   "A mask made with egg yolks dries out the skin.",
                   "A fast fix for removing oil shine requires one of the simplest foods: the egg. An egg yolk mask dries out the skin. Apply the egg yolk with a cotton ball to oily spots. Leave on for 15 minutes, then rinse with cool water.",
                   group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-7",
                  "7. From the Home Remedies Cupboard",
                  "Dry Skin",
                  "Assets/Healthy-Eating-In-The-Usa.jpg",
                  "The home remedies found below are easy to locate in your own home kitchen, and will relieve you from some of the discomfort that comes from dry skin..",
                  "Baking soda. \nInstead of using an abrasive dishwashing cleanser, try sprinkling skin-friendly baking soda in your dishwater. Baking soda is also a skin-friendly alternative to jumping in a hot shower. Try a sponge bath using 4 tablespoons baking soda to 1 quart water. A baking soda soak is a folk remedy to relieve itching. Add 1 cup baking soda to a tub of hot water. Soak for 30 minutes and air dry. \nCornstarch.\n You may think cornstarch can only be used to thicken your gravy, but it's also useful in easing itchy, dry skin. Sprinkle a handful in the bathtub and have a soak. \nOatmeal. \nAdding instant oatmeal to your bath will soothe your skin. The oats are packed with vitamin E, a nutrient vital to healthy skin. Oatmeal is also used as a folk remedy for treating dry, chapped hands. Rub your hands with wet oatmeal instead of soap. Dry your hands with a towel, then rub them with dry oatmeal. \nSalt. \nMassage a handful of salt onto wet skin after a shower or bath. It will remove dry skin and make your skin smooth. \nVegetable oil. \nCoating yourself with vegetable oil may make you feel like a French fry, but your skin will love you. In fact, experts say that any oil, from vegetable to sunflower to peanut, offers relief from dry skin. \nVinegar. \nTry this folk remedy for chapped hands: Wash and dry hands thoroughly, then apply vinegar. Put on a pair of soft gloves and leave them on overnight..",
                  group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-8",
                 "8 .Do Remember",
                  "Dry Skin",
                  "Assets/Day-creams.jpg",
                  "Put some water on for a slow boil to raise the humidity in your house.Take lukewarm or cool showers.",
                  "Be cool.\n Take lukewarm or cool showers. This may not sound very appealing if you like lounging in the hot steam, but your skin will thank you. Hot water draws out skin's valuable oils, which will dry out your skin. \nBe selective about soap.\n Pretty, perfume-laden soap may look and smell nice, but it can leave your skin screaming. Try soaps with fat or oil in them, such as Dove or Basis. Liquid soaps tend to be milder than bar soaps.\nDouse while you're still damp.\n Slathering lotion on damp skin is your best bet for retaining moisture. When you get out of the bath or shower, pat, don't rub, to get rid of just enough water so you don't leave a wet trail to the sink. Then spread on your lotion while you've still got droplets clinging to your skin. This will help seal in the moisture. \n Avoid alcohol.\n That means both the kind you drink and the kind you use to cleanse. Drinking alcohol can cause your body to soak up water from skin. Limit yourself to no more than 2 ounces a day to keep your skin healthy. Alcohol-based cleansing products (such as astringents) dry out your skin, too. It's best to skip them altogether. \nWatch the sun. \nYou put your wet tennies outside to dry out. Well, just as the sun evaporates moisture from your water-soaked shoes, it evaporates moisture from your skin. Though a little bit of that evaporation is healthy (sweat evaporating keeps you cool when you exercise), too much can be a problem. So protect your skin by wearing sunscreen and moisturizing lotions if you spend lots of time in the sun. \nRehydrate your skin with lotion after using any degreasers or solvents when painting around the house. Just a few simple home remedies could have you feeling smooth in no time, and ready to take on the worst the sun and wind can throw at you..",
                  group1));
            
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                    "Digestion Problems",
                    "Herbal Remedies for Digestive Problems",
                    "Assets/digestion.jpg",
                    "Digestive problems can be helped enormously by herbal remedies. There are plants to stimulate digestion or relax it, to help expel gas, and to soothe inflammation and pain. Most culinary herbs were used because of their ability to facilitate digestion.");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Herbal Remedies",
                    "Top 5 Herbal Remedies",
                    "Assets/natural-herbs-500x500.jpg",
                    "While all of our herbal remedies can have spectacular results, some herbs appear to get more attention than others. .",
                    "While all of our herbal remedies can have spectacular results, some herbs appear to get more attention than others. Below are some of the most popular herbs in health-food stores and supermarkets: \n1. Chamomile is a popular variety of tea, but the chamomile plant is also used in a number of herbal remedies. To lean how to use chamomile to treat anxiety, cramping, and muscle pain, read Chamomile: Herbal Remedies. \n 2. While echinacea was used centuries ago by the Native Americans, today many people take this herb to help fight off colds. Learn more about how echinacea can boost your immune system in Echinacea: Herbal Remedies. \n 3. Ginkgo, or ginkgo biloba, has received a lot of attention lately for its ability to improve circulation and brain activity. Learn more in Ginkgo: Herbal Remedies. \n 4. Ginseng has been used for thousands of years, but has recently found favor with a public looking for a natural energy-boost. Find out more in Ginseng: Herbal Remedies. \n 5. Many people have looked to St. John's wort as an herbal alternative to prescription medications for anxiety and depression. St. John's Wort: Herbal Remedies will tell you if this herb has what it takes to relieve depression.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Yogurt , Apple Juice",
                    "Keeps Colon Healthy",
                    "Assets/yogurt.jpg",
                    "Eating fresh yogurt on a regular basis is a good way to keep the colon healthy.",
                    "Eating fresh yogurt on a regular basis is a good way to keep the colon healthy. Yogurt will give your body pro-biotic and good bacteria and will get rid of bad bacteria. It also contains a good amount of calcium which discourages the growth of cells lining the colon. You can eat yogurt as it is or add some fresh fruits such as apples, limes, banana and so on to it. Yogurt is good for an infected colon and at the same time it also solves various stomach problems such as indigestion, flatulence, irregular bowel movements and lots more. \n Apple Juice \n Fresh apple juice is one of the best home treatments for colon cleansing. Regular intake of apple juice encourages bowel movements, breaks down toxins and improves the health of liver as well as the digestive system. Start your day with one glass of unfiltered apple juice and then after half an hour drink one glass of water. Throughout the day follow this routine several times. In between you can also drink one glass of prune juice. Freshly squeezed apple juice should be used to get good results, but if it is not available then you can use organic apple juice. When following this remedy it is advisable to avoid solid food",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                    "Fiber Foods , Water",
                    "Cleanse the colon",
                    "Assets/fiberfood.jpg",
                    "Foods rich in fiber should be consumed in high amount to cleanse the colon of the harmful toxins. ",
                    "Foods rich in fiber should be consumed in high amount to cleanse the colon of the harmful toxins. Fiber will help to keep the stools soft and improve the bowel movement which ultimately encourages the body to expel waste products. At the same time fiber enriched foods will also help to get rid of any kind of intestinal problems. You can add a lot of fiber into your diet by eating fresh fruits like raspberries, pears and apples, as well as fresh vegetables like artichokes, peas and broccoli. Cereals, whole grains, nuts, beans and seeds also contain a good amount of fiber.\n Water \n For colon cleansing, the best thing that you can do is to drink plenty of water. It is essential to drink at least ten to twelve glasses of water in a day. Regular consumption of water will give your body the liquid and lubrication required to flush out the harmful toxins and waste from the body in a very natural manner. Drinking plenty of water will also stimulate the natural peristaltic action and soon the colon will start functioning in a normal manner. Also water is essential to keep your body well hydrated. Along with water you can also drink fresh fruit and vegetable juices.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-4",
                    "Lemon Juice , Raw Vegetable Juice",
                    "Antioxidant properties",
                    "Assets/juice.png",
                    "Lemon has antioxidant properties and its high vitamin C content is good for the digestive system. ",
                    "Lemon has antioxidant properties and its high vitamin C content is good for the digestive system. Hence for colon cleansing you can try lemon juice. Take the juice of one lemon and add a pinch of sea-salt, little honey and one glass of lukewarm water to it. Mix all the ingredients properly and drink this solution on an empty stomach in the morning. This will help you to enjoy more energy, better bowel movement and better skin condition. During the day time you can add two tablespoon of fresh squeezed lemon juice to one glass of apple juice and drink it three to four times a day. This will thin out the mucous in the bowel. \n Raw Vegetable Juice \nFor colon cleansing, it is essential to keep away from processed and cooked food for one or two days. Instead of solid food, try to drink fresh vegetable juice several times a day. Vegetable juices contain sugars that can improve the bowel movement. Also the vitamins, minerals, amino acids and enzymes present in it will keep your body healthy and well energized. It is advisable not to use readymade vegetable juices as it does not contain the effective enzymes that help your body to break down and remove waste products. You can easily make fresh vegetable juice at home with the help of a juicer or blender.",
                    group2));
            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                    "Eye Problems",
                    "Home Remedies To Improve Eyesight",
                    "Assets/eyeproblems.jpg",
                    "Your eyes are the windows to the world around you. However, what happens when the picture becomes blurry or unclear? The fact is that your vision provides up to 80 percent of your total sensory input. With this in mind, you can clearly see why it is so important to ensure optimum eye health.");
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                    "Eye Exercises",
                    "Strengthen The Eye Muscles ",
                    "Assets/eye.jpeg",
                    "The following eye exercises work to strengthen the eye muscles and maintain the flexibility of your eye lens in order to improve your vision. Some of the most popular exercises include:",
                    "Breathing Yoga Exercises: This will encourage conscious breathing. The point of this is to relax your body and close your eyes. This allows the muscles to relax. Breathe in through the nose and out through the mouth for two minutes. When you finally open your eyes after two minutes, do not focus on anything. Repeat the exercise three times. \nBlinking Exercises: \nEach time you blink you are soothing and moisturizing your eyes. This can be done anywhere if your eyes are feeling weary or tired. Blink consistently for 4 to 5 seconds at a time for a period of two minutes. \nPalming Exercises: \n These exercises were specifically designed to stimulate the various acupuncture points around your eyes. This aid in relaxing the muscles in and around your eye and helping to calm the mind. To execute this exercise you should place your elbows on a flat surface in front of you with your left palm covering your left eye. Your fingers should be resting on your forehead. Do the same with your right eye and right palm. The palms should cup the eyes, but not put any pressure on them. You should do this exercise for a period of three minutes. \n The Rebuild Your Vision Program: \n Included with the Rebuild Your Vision Program is 8 step-by-step vision improvement eye exercises. When these 8 eye exercises are performed together, they can start to dramatically improve your vision in less than 48 hours. Along with the 8 eye exercises comes the 7 essential keys to vision improvement success. Just one or two of these keys could prevent stronger prescriptions ever again. The 8 eye exercises and 7 vision improvement techniques come in easy to follow daily training routines. This is like having your own personal trainer ensuring that your time isn’t wasted on eye exercises that don’t apply to your vision problems.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                    "Diet",
                    "Vision-Enhancing Foods",
                    "Assets/protein-diet-plan.jpg",
                    "It is important to eat the proper diet to promote eye health. This includes the addition of certain vision-enhancing foods if you currently do not consume them:",
                    "Bilberries: Fruit that is rich and full of antioxidants. These will aid in protecting the eyes from free radicals. They will also aid in better blood circulation in your eyes. \nGrapes and Blueberries: \n These will aid your night vision. \nCarrots: \nRich in beta-carotene, which is known for great vision, as well as vitamin A. \nCold Water Fish:\n Fish is rich in DHA, which will provide support to the structure of the cell membranes in your eyes. It is also used for many different treatments for disorders such as macular degeneration, dry eyes and general treatment of eyesight. \nLeafy Green Vegetables: These have nutrients that will help to protect your eye’s macula, due to their rich carotenoid content.\nCapers, Shallots, Garlic, Eggs and Onions: \nThese also contain helpful antioxidants that are beneficial for the lens of your eyes. \nIf you want to ensure that your eyes are getting the 17 essential nutrients and vitamins they need for optimal eye health, check out  the best eye vitamin on the market.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                    "Lifestyle Habits",
                    "Proper Eye Health",
                    "Assets/lifestyle.jpg",
                    "When it comes to eye health, you must treat them as any other system in your body. This means providing exercise, and the proper nutrients.",
                    "When it comes to eye health, you must treat them as any other system in your body. This means providing exercise, and the proper nutrients. However, these are not the only elements that aid proper eye health. \nIn addition to the tips listed here, you also need to ensure that you receive plenty of sleep each day. You need to try to achieve six to eight hours each night to allow your eyes to fully rest and rejuvenate. If you do not get enough sleep, you may find that you have an abnormal amount of eye health issues. \n Another important factor is to drink plenty of water. The body is composed of mainly water, meaning that you must replenish it regularly. This will help provide moisture to cleanse dust and debris from your eyes throughout the day. Water is essential for all of the body’s functions, including proper and healthy eyesight. In most cases drinking eight, eight-ounce glasses of water a day will provide enough moisture and water for your body.\nHowever, if you live in a dry area, more is necessary to provide optimum eye health. \nTaking care of your eyes is a life-long process. This includes preventing eye health issues, as well as the need to wear corrective lenses. There are thousands of success cases and studies that tell the benefits of proper eye health and preventative measures that you can take at home. The home remedies and lifestyle changes here will make a huge difference in the overall eye health that you have. \nIt is important to teach them to your child, as well.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-4",
                    "Eyesight Enhancing Tips",
                    "Prevent Eye Problems",
                    "Assets/eyesigh.jpg",
                    "The fact is that it is much smarter to prevent eye problems, rather than treat them after the problem is present. While eye exercises and a healthy diet are important to your overall eye health, there are other tips you can use that will help to naturally improve your eyesight. These include:",
                    "Minimize the extended period of time that you spend in front of the computer screen. Be sure to use our 10-10-10 rule while on the computer. \nAfter every half-hour of television watching take a 10 minute break. This will provide a break for your eyes. \nTry to look at objects far away when you take a break from in front of the TV or computer screen. \nWhen your eyes feel tired or worn out, try washing them with cold water. \nWhen they feel extremely tired, close them for a while to allow them to rest. The old saying is “take 5 to stay alive”.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-5",
                    "Ayurvedic Tips",
                    "Traditional Medicine",
                    "Assets/ayurved.png",
                    "This is a simple and influential remedy. After using it, eyesight would improve significantly and you would became grateful to Ayurveda.",
                    "First step:- Before preparing it, we would like to discuss about the things that’s are needed.\nIngredients: - \n•	Almond (Badam)\n•	Large Fennel (Sounf)\n•	Sugar or (Kuja Mishri) is also known as Crystallized Sugar lumps.\nNote:- You may use either Kuja Misri (Crystallized Sugar lumps) or Sugar.\nMethod To Prepare the Remedy: -\nTake equal amount of almond, large fennel and sugar (Kuja mishri) and make the fine powder by grinding these. Keep the mixture in a glass vessel. Now the remedy is ready for use. \nTreatment Method: -\nTake 10 gram with 250 ml of milk at every night before you go to sleep. Use continuously 40 days, then your eyes will become brighter and doesn't need glasses. This is true, if you don't believe then should try to feel the effects.\nIt has also cured from weakness of mind, twisted mind, and amnesia by this remedy.\nPrecautions: -\n•	Give half quantity of medication to the minors.\n•	For the full benefit of the remedy, don’t drink water for two hours after taking the remedy.\nSpecial Advise:-\n•	Cow's milk is more beneficial.\n•	Should drink amla juice daily morning.\n•	You should keep water in mouth, When you are wash your face. And should be do spraying water on eyes.\n",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-6",
                    "Preventive Tips",
                    "Maintaining Good Eye Health",
                    "Assets/lemon.jpg",
                    "Don't take your eye health for granted. Protect your eyesight with these six tips.",
                    "1. Eat for Good Vision\nProtecting your eyes starts with the food on your plate. Studies have shown that nutrients such as omega-3 fatty acids, lutein, zinc, and vitamins C and E may help ward off age-related vision problems such as macular degeneration and cataracts. \nRegularly eating these foods can help lead to good eye health:\nGreen, leafy vegetables such as spinach, kale, and collards Salmon, tuna, and other oily fish,Eggs, nuts, beans, and other non-meat protein sources Oranges and other citrus fruits or juices Eating a well-balanced diet also helps you maintain a healthy weight, which makes you less likely to get obesity-related diseases such as type 2 diabetes. Diabetes is the leading cause of blindness in adults. \n2. Quit Smoking for Better Eyesight\nSmoking makes you more likely to get cataracts, optic nerve damage, and macular degeneration. If you've tried to quit smoking before and started smoking again, keep trying. Studies show that the more times you try to quit smoking, the more likely you are to succeed.  \n3. Wear Sunglasses for Good Vision\nThe right kind of sunglasses will help protect your eyes from the sun's ultraviolet (UV) rays. Too much UV exposure makes you more likely to get cataracts and macular degeneration.Choose sunglasses that block 99% to 100% of both UVA and UVB rays. Wraparound lenses help protect your eyes from the side. Polarized lenses reduce glare when driving. If you wear contact lenses, some offer UV protection. It's still a good idea to wear sunglasses for more protection. \n4. Use Safety Eyewear at Home, at Work, and While Playing Sports If you work with hazardous or airborne materials at work or home, wear safety glasses or protective goggles every time. \nCertain sports such as ice hockey, racquetball, and lacrosse can also lead to eye injury. Wear eye protection (such as helmets with protective face masks or sports goggles with polycarbonate lenses) to shield your eyes.\n5. Look Away From the Computer for Good Eye Health Staring at a computer screen can cause: \nEyestrain \nBlurry vision \nDifficulty focusing at a distance \nDry eyes \nHeadaches \nNeck, back, and shoulder pain \nProtect your eye health by taking the following steps: \nMake sure your glasses or contact lens prescription is up-to-date and adequate for computer use. Some people may need glasses to help with contrast, glare, and eye strain when using a computer. Position your computer so that your eyes are level with the top of the monitor. This allows you to look slightly down at the screen. Try to avoid glare on your computer from windows and lights. Use an anti-glare screen if needed. Choose a comfortable, supportive chair. Position it so that your feet are flat on the floor. If your eyes are dry, blink more. Every 20 minutes, rest your eyes by looking 20 feet away for 20 seconds. At least every two hours, get up and take a 15-minute break.",
                    group3));
           
            this.AllGroups.Add(group3);

            var group4 = new SampleDataGroup("Group-4",
                    "Dental Problems",
                    "Herbal Remedies for Dental Problems",
                    "Assets/dental.jpg",
                    "Inflamed or bleeding gums, gingivitis, thrush, bad breath, toothaches, and teething are some of the conditions that plague the mouth. None are comfortable.");
            group4.Items.Add(new SampleDataItem("Group-4-Item-1",
                    "Herbal Remedies",
                    "Calendula And Echinacea.",
                    "Assets/calendula.jpg",
                    "Calendula and echinacea soothe sore gums and reduce inflammation. They are an excellent treatment for Candida albicans, an opportunistic yeast that causes thrush in the mouth.",
                    "Calendula and echinacea soothe sore gums and reduce inflammation. They are an excellent treatment for Candida albicans, an opportunistic yeast that causes thrush in the mouth. Dab affected areas with calendula tincture diluted with an equal amount of water. For gums, make a strong infusion and swish it in the mouth for several minutes. You can either spit it out or swallow it. A couple of drops of lavender oil is also good at clearing up Candida albicans, as well as reducing inflammation and healing sores. Infected gums are successfully treated with goldenseal or Oregon grape, too. Their berberine content gives them antimicrobial effects, killing off offending bacteria. Rosemary helps heal canker sores and has antiseptic properties. Parsley is a natural breath sweetener. Instead of sending it back to the kitchen on your plate, nibble this herb after your meal. Not only will you have better breath, but you'll also get a boost of vitamins A and C and the minerals calcium and iron. Licorice sweetens breath, too, and is frequently used to flavor and sweeten herbal toothpaste and mouthwashes. Fennel has a similar action and will also help if you have gas.Oil of cloves, a tropical spice you can't grow in your garden, is good to have on hand for toothaches or teething. Rub a little on the gums. Whatever your mouth affliction, natural herbs such as calendula and echinacea can help offer relief.",
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-2",
                    "Oral Hygiene.",
                    "Steps",
                    "Assets/oral.jpg",
                    "We'll elaborate on that elusive virtue called oral hygiene -- specifically, brushing your teeth, flossing, whitening teeth, eating an anticavity diet, and visiting the dentist. Here's a preview:.",
                    "How to Brush Your TeethBrushing your teeth is important to remove stray food particles, massage the gums, eliminate plaque, and freshen breath. It also helps to defend against cavities and periodontal disease. Proper brushing technique is just as important as vigilance -- be sure you bone up on the proper way to wield that toothbrush. \nHow to Floss Your TeethRegular flossing is your best defense against gum disease, and it also fights cavities and decay. Good flossing technique will help you remove debris between the teeth, preventing gingivitis and periodontitis. Using the right floss and dental-cleaning equipment will aid in this process. \nHow to Whiten Your TeethSome stains can be cleaned by regular visits to the dentist, while others require more aggressive measures. Brushing and flossing regularly is the best way to prevent stains. Professional bleaching can be effective in returning your teeth to a glimmering state. \nAn Anticavity DietTo promote strong, heathy teeth, eat a diet rich in calcium, vitamin D, vitamin C, and fluoride. Simple sugars and starchy foods are a banquet feast for oral bacteria. A diet that is full of sugars and overprocessed foods (or one devoid of vitamins, minerals, and crunchy fruits and vegetables) can eventually lead to decay, even in the mouths of the most avid brushers and flossers. \nVisiting the DentistDon't let anxiety keep you away from the dentist's office -- the dentist is an important partner in maintaining your oral health. During a professional cleaning, the dentist or hygienist removes tartar and polishes the surface of your teeth, making it harder for plaque and tartar to adhere to them. The dentist then thoroughly examines your teeth and gums to search for any problem areas.",
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
                    "Treating with Home Remedies",
                    "Good Oral Health ",
                    "Assets/good.jpg",
                    "The knowledge of these simple things will help you to maintain a good oral health for yourself. This will also facilitate in bringing awareness about the healthy gums and teeth and how one can use the easiest ways at home.",
                    "1. Reduce stress. According to the Academy of General Dentistry (AGD), there's a link between stress and your dental health. People under stress have a compromised immune system that makes it harder for them to fight off the bacteria that causes periodontal disease and makes them more prone to gum infection. Researchers have also learned that not all stress is created equal. In studies done at three different U.S. universities, participants experiencing financial worries were at greatest risk for periodontal disease.\n2. Make a sea salt solution. Dissolve a small amount of sea salt in a cup of warm water. Swish a sip of the solution in your mouth for 30 seconds and spit it out. Repeat several times. Salt water will reduce swollen gums and draw infection out of any abscesses. Add this mouth rinse to your twice-daily brushing routine. \n3. Apply tea bags. Steep a tea bag in boiling water, remove and allow it to cool until you can handle it comfortably. Hold the cooled tea bag on the affected area of your gums and keep it there for about five minutes. The tannic acid in the tea bag can work effectively to relieve gum infection. \nDirectly applying the tea bag to your gums is more effective than simply drinking the beverage. Plus, drinking too much tea has a dental downside: discolored, tea-stained teeth. \n4. Rub some honey. Honey has natural antibacterial and antiseptic properties, so you can put it to work treating your infected gums. Once you brush your teeth, rub a small amount of honey on the problem area of your gums. \nGiven honey's high sugar content, you want to be careful you don't overapply it and do your best to put it on your gums only rather than on your teeth. \n5. Drink cranberry juice. Cranberry juice can prevent bacteria from sticking to your teeth, so try drinking up to 4 ounces of the unsweetened juice daily. \n6.Make a lemon paste. Make a paste from the juice of one lemon and some salt. Mix it well and apply to your teeth. Let it sit for a few minutes and gargle with warm water to rinse it off. \nLemons offer a win-win solution for treating gum disease. First, they're an anti-inflammatory, which makes them helpful in treating infected gums. Not only that, but lemons contain vitamin C, which can help your gums fight off infection.",
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-4",
                    "Using Drug Store Remedies",
                    "Growth And Bone Regeneration",
                    "Assets/image066.jpg",
                    "Vitamin C is an antioxidant, and antioxidants are found to promote connective tissue growth and bone regeneration, which can be affected by various gum problems.",
                    "Eat more C-rich foods. It's not just lemons that can help with gum disease, but other foods full of vitamin C such as oranges, grapes, kiwi mango, papaya and strawberry are good choices, too. Vitamin C is an antioxidant, and antioxidants are found to promote connective tissue growth and bone regeneration, which can be affected by various gum problems. \nIncrease your intake of vitamin D. Vitamin D has anti-inflammatory properties, so be sure you're getting enough of this vitamin when you're trying to heal swollen gums and prevent the condition from reoccurring. Older adults should particularly take note of this vitamin. According to the National Institutes of Health, higher blood levels of vitamin D seem to be linked to a reduced risk of gum disease in people age 50 and older.Get your vitamin D fix by soaking up the sun at least 15 to 20 minutes twice a week and eating D-rich foods such as salmon, whole eggs and cod liver oil. \nBrush with baking soda. Baking soda neutralizes the acids in your mouth thereby reducing the chances of tooth decay and gum disease, so it's more of a preventative measure than an actual treatment for gum disease. Add a small amount of baking soda to a bit of warm water and mix to form a paste. Use this paste to brush your teeth. \nGive up tobacco. Tobacco decreases your ability to fight infection and delays healing. Tobacco users are more likely than nonsmokers to have serious gum disease that doesn't respond as well to treatment and that leads to tooth loss. ",
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-5",
                    "Tooth Care",
                    "Massage Your Gums",
                    "Assets/toothcare.JPG",
                    "Brush your teeth with turmeric and coconut oil mixed togather to form a paste. apply it to toothbrush and brush your teeth at night.",
                    "Brush your teeth with turmeric and coconut oil mixed togather to form a paste. apply it to toothbrush and brush your teeth at night. In the morning do not brush, only massage your gums. Coconut oil is anti bacterial. this treatment may be done once a week to have healthy shining teeth.(can also be followed daily). \nFor those who have read some of the absurd totally misleading posts on whitening your teeth with baking soda. PLEASE be very careful when treating your teeth this way! First of all if using baking soda you must use only a very small amount and surely not every day of the week. A high concentration of baking soda rips the enamel right off your teeth. \nTo be safer than sorry you can instead use a toothpaste sold in stores that already contains the correct amounts of hydrogen peroxide and baking soda (I use arm&hammer for example). But whatever you do please do a CAREFUL research about the baking soda effects and how long you should repeat the procedure...there are many articles online and posts by doctors that you can check. \nFOR TEETH PROBLEMS HOME REMEDY:- Take little black peper powder, turmeric powder,and black rock salt. Mix it nicely and brush the teeeth through ur fingure massaging the tooth very gently.Regular use of this home made powder will keep ur teeth healthy and shine. \n For whiter teeth I brush my teeth with salt.Then I use a BANANA peel and rub it all over my teeth (use the inside part!) and let that sit for a minute or so, lick it off :) and then brush my teeth again with a sprinkle of salt on my toothpaste. The salt will make your gums tingle or sting a bit so don't be too rough. Aftershave my teeth feel VERY clean. In the morning I wake up with white teeth! I have only done this once and I think I will do this every night. Try it and let me know how it works and when you do it! Good luck ",
                    group4));
            group4.Items.Add(new SampleDataItem("Group-4-Item-6",
                    "Remedies for Bad Breath ",
                    "Steps To Follow",
                    "Assets/Bad_breath_cartoon.jpg",
                    "This is the most common and most embarrassing condition. One who has bad breath really wants to get rid of it.",
                    "Deposits on the tongue are one of the major causes of bad breath. One should regularly clean the tongue with the tongue scrapper or the brush to clean the deposits present on the tongue.\nThe persons who have dry mouth usually have the problem of bad breath because in the dry mouth case the cleansing action of the saliva is missing. They can chew the sugarless chewing gum to increase the salivary flow. Some special chewing gums have anti odour ingredients which are also very useful. \nChewing fennel seeds, parsley or cinnamon sticks are also considered to be quite effective for bad breath and is a good home remedy. \nUsing antimicrobial mouth wash before going to bed at night is also effective against the bad breath. \nIf one is a denture wearer whether it is full or partial, one should clean it properly. They can be washed with mild detergent. \nOne can boil 4 to 6 leaves of fenugreek seeds in a glass of water and can use it as the mouth wash. It really helps. This can be used twice a day for minimum 7 days to get the best results. \nBrush your teeth daily minimum two times a day. Once in the morning after breakfast and once at bed time after dinner. You should also floss your teeth regularly so that the food impaction is not there. \nDo warm saline rinses two to three times a day. You can use 2 % hydrogen peroxide diluted with water in the ratio of 1:4 as the mouth wash. \nYou can use baking soda with water. Apply it on the gums and then brush your teeth. Baking soda helps in decreasing the number of microbes in the mouth. \nSome times the problem of bleeding gums is because of vitamin-C deficiency. It is also called as scurvy. In this case consume citrus fruits. Lemon water without sugar is also good. \nBecause of its good healing properties one can use aloe on the gums and teeth. \nOne can add a drop of tea tree oil in the tooth paste to prevent the occurrence of gum disease. ",
                    group4));
            this.AllGroups.Add(group4);

            var group5 = new SampleDataGroup("Group-5",
                    "Bone Injuries",
                    "Fractures , Fingertip Injuries, Leg Injuries, Hand Fracture ",
                    "Assets/injuries.jpg",
                    "Here are some of the home remedies for Fractures , Fingertip Injuries, Leg Injuries, Hand Fracture which people can follow at their home .");
            group5.Items.Add(new SampleDataItem("Group-5-Item-1",
                    "Home Remedies Bone Fracture",
                    "Regain Bone Strength",
                    "Assets/strength-training1.jpg",
                    "When a fracture occurs, cause terrible pain and tenderness in the area fractured, along with swelling, the appearance of some blood under the skin and some numbness, tingling or paralysis below the injured area.",
                    "1.Eat half pineapple every day until it’s completely healed. It contains Bromelain, an enzyme that helps to reduce swelling and inflammation. Do not eat canned or processed pineapples. If you don’t  like fresh pineapple, take the supplement Bromelain. It has the same effect as pineapple. \n2.  It is very important to regain Bone Strength as soon as possible to avoid future injuries and to insure a solid bone fusion. we recommend that you take this.\n3. Do not eat red meat, and avoid drinking colas and all products containing caffeine.\n4. Avoid eating foods with preservatives, they contain Phosphorous which can lead to bone loss. \n5. Take Boron, is important for the health and healing of the bone.\n6. Take Calcium + Magnesium + Potassium.  They are essential to repair bone damage and to maintain a good muscle and heart condition. 7. Take Zinc, helps repair tissue damage.",
                    group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-2",
                    "Fingertip Injuries",
                    "How are they treated?",
                    "Assets/finger.png",
                    "Injured components may include skin, bone, nail, nailbed, tendon, and the pulp, the padded area of the fingertip.",
                    "Severe crush or avulsion injuries can completely remove some or all of the tissue at the fingertip. If just skin is removed and the defect is less than a centimeter in diameter, it is often possible to treat these injuries with simple dressing changes. If there is a little bit of bone exposed at the tip, it can sometimes be trimmed back slightly and treated with dressings, too. For larger skin defects, skin grafting is occasionally recommended. Smaller grafts can be obtained from the little finger side of the hand. Larger grafts may be harvested from the forearm or groin. If the nailbed is injured, it is repaired (see web page on nailbed injuries).When patients lose more than skin and have exposed bone, the injury may need to be covered with a flap of skin that has some soft tissue with it for padding. Small wounds on the tip of the finger may be covered with a flap from the injured finger. Larger wounds, such as those that result in substantial loss of the pulp, require a flap that is elevated from an adjacent finger (see Figure 2) or other source. The flap remains attached to its original site so that it has blood supply while it is stitched to the finger wound. A skin graft is used to cover the donor site defect. After a few weeks the flap has sufficient blood supply coming from the injured finger as it heals into its new location, and can be divided from its origin and fully set into the wound.",
                    group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-3",
                    "Leg Injuries",
                    "Treatment",
                    "Assets/leg.jpg",
                    "When too much stress is placed on a joint or other tissue, often by overdoing an activity or doing the same activity repeatedly.",
                    "Treatment for a leg injury may include rest, ice, elevation, and other first aid measures (such as the application of a brace, splint, or cast), or Physiotherapy. Some leg injuries are treated with medicine or surgery, especially if a bone is broken. Treatment depends on: \nThe location, type, and severity of the injury. \nWhen the injury occurred. \nYour age, health condition, and activities, such as work, sports, or hobbies. Minor leg injuries are common. Symptoms often develop from everyday wear and tear, overuse, or an injury. Leg injuries are most likely to occur during: \nSports or recreational activities.\nWork-related tasks.\nWork or projects around the home.\nMost leg injuries in children and teens occur during sports or play or from accidental falls. The risk for injury is higher in contact sports, such as wrestling, football, or soccer, and in high-speed sports, such as biking, in-line skating, skiing, snowboarding, and skateboarding. Knees, ankles, and feet are the most affected body areas. Any injury occurring at the end of a long bone near a joint may injure the growth plate and needs to be checked by a doctor. \nOlder adults have a higher risk for injuries and fractures because they lose muscle mass and bone strength (osteoporosisClick here to see more information.) as they age. They also have more problems with vision and balance, which increases their risk for accidental injury. Most minor injuries will heal on their own, and home treatment is usually all that is needed to relieve symptoms and promote healing.Acute (traumatic) injury An acute injury may occur from a direct blow, a penetrating injury, a fall, or from twisting, jerking, jamming, or bending a limb abnormally. Pain may be sudden and severe. Bruising and swelling may develop soon after the injury. Acute injuries usually require prompt medical evaluation and may include:\nBruises (contusionsClick here to see an illustration.), which occur when small blood vessels under the skin tear or rupture, often from a twist, bump, or fall. Blood leaks into tissues under the skin and causes a black-and-blue colour that often turns colours, including purple, red, yellow, and green, as the bruise heals.\nInjuries to the tough, ropey fibres (ligaments) that connect bone to bone and help stabilize joints (sprainsClick here to see more information.).\nInjuries to the tough, ropey fibres that connect muscle to bone (tendons), such as a ruptured Achilles tendonClick here to see more information.\nPulled muscles (strainsClick here to see more information.), such as a hamstring strain.\nMuscle rupturesClick here to see an illustration., such as gastrocnemius rupture.\nBroken bones (fracturesClick here to see more information.). A break, such as a lower leg fractureClick here to see an illustration., may occur when a bone is twisted, bent, jammed, struck directly, or used to brace against a fall.\nPulling or pushing bones out of the normal relationship to the other bones that make up a joint (dislocationsClick here to see more information.).",
                    group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-4",
                    "Hand Fracture",
                    "How is a hand fracture treated?",
                    "Assets/handfra.jpg",
                    "A hand fracture is a break in one of the bones in your hand. Your hand is made up of bones called phalanges and metacarpals.",
                    "Brace, cast, or splint: A brace, cast, or splint may be used to decrease your hand movement. These work to hold the broken bones in place, decrease pain, and prevent further damage to your hand. \nFinger strapping: If you have a broken finger, your broken finger may be strapped or taped to the finger next to it. This can provide support, limit motion, and decrease stiffness.\nMedicine:Pain medicine: You may be given a prescription medicine to decrease pain. Do not wait until the pain is severe before you take this medicine.\nAntibiotics: This medicine is given to help treat or prevent an infection caused by bacteria.\nTetanus shot: This is a shot of medicine to prevent you from getting tetanus. You may need this if you have breaks in your skin from your injury. You should have a tetanus shot if you have not had one in the past 5 to 10 years.\nSurgery: If you have an open fracture, you may need debridement before your surgery. This is when your caregiver removes damaged and infected tissue and cleans your wound. Debridement is done to help prevent infection and improve healing.\nExternal fixation: In this surgery, screws may be put through your skin and into your broken bones. The screws will be secured to a device outside of your hand. External fixation holds your bones together so they can heal. It is often done if you have severe tissue damage or many injuries.\nOpen reduction and internal fixation: Your caregiver will make an incision in your hand to straighten your broken bones. He will use screws and a metal plate, nails, or wires to hold your broken bones together. This surgery will allow your bones to grow back together.\nPin fixation: With this surgery, metal pins will be used to straighten the broken bones in your hand. The pins will hold the broken pieces of bone together. Your caregiver will place the pins through your skin and into your bone using a small drill.",
                    group5));
            this.AllGroups.Add(group5);

            var group6 = new SampleDataGroup("Group-6",
                    "Body Pains",
                    "Muscle Pain, Lower Back Pain , Aches , Shoulder Pain",
                    "Assets/body_aches.jpeg",
                    "Here are some of the home remedies for Muscle Pain, Lower Back Pain , Aches , Shoulder Pain  which people can follow at their home .");
            group6.Items.Add(new SampleDataItem("Group-6-Item-1",
                    "Muscle Pain",
                    "Treatments For Muscle Pain",
                    "Assets/muscle.jpg",
                    "Ease those muscle cramps and other muscular aches and pains by following the home remedies below..",
                    "Stop. If your muscle cramps up while you're exercising, STOP. Don't try to run through a cramp. Doing so increases your chances of seriously injuring the muscle.\nGive it a stretch and squeeze. When you get a cramp, stretch the cramped muscle with one hand while you gently knead and squeeze the center of the muscle (you'll be able to feel a knot or a hard bulge of muscle) with the fingers of the other hand. Try to feel how it's contracted, and stretch it in the opposite direction. For example, if you have a cramp in your calf muscle, put your foot flat on the ground, then lean forward without allowing your heel to lift off the ground. If you can't stand on your leg, sit on the ground with that leg extended, reach forward and grab the toes or upper portion of the foot, and pull the top of the foot toward the knee.\nWalk it out. Once an acute cramp passes, don't start exercising heavily right away. Instead, walk for a few minutes to get the blood flowing back into the muscles.\n",
                    group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-2",
                    "Lower Back Pain",
                    "Chronic Condition",
                    "Assets/low.jpg",
                    "Getting it under control can improve your productivity as well as your relationships..",
                    "Ease the ache: Find a healing touch. Recent research shows that massage therapy may be more effective than commonly used treatments such as drugs for short-term relief from soreness in your lower back, according to the Annals of Internal Medicine. After 10 weeks, study participants who received either a 1-hour relaxing Swedish massage or a form of massage focused on treating the tissues of the back were twice as likely to have spent fewer days in bed, taken fewer meds, and continued everyday activities (think: walking up stairs or having sex) compared with those who got usual care, such as painkillers, anti-inflammatory drugs, or muscle relaxants. Moreover, benefits lasted more than 6 months. This study showed that massage is not only helpful in relieving lower-back pain, but you can get the same benefits with the more commonly available relaxing massage as you can with a more targeted massage, says lead study author Dr. Cherkin.Experts aren't sure how massage works exactly to relieve back pain, but one theory might be that spending an hour being touched in a relaxing environment calms the central nervous system to lessen the perception of pain. To find a trained massage therapist near you, check out the American Massage Therapy Association. Massage may not be covered by your insurance even for medical reasons, but an affordable option is to find a teaching school near you. Relaxing massages can cost only $35 for 50 minutes, and students are under the guidance of a licensed massage therapist. \nPain Hotspot #2: Knees. You may not think much about your knees until you feel shooting pain while walking up stairs or throbbing after being on the go all day. While there are many reasons for knee pain, a common trigger is plain old wear and tear from arthritis. Being overweight puts added pressure on your knees, so losing pounds can help. \nEase the aches: Keep moving. Though it may seem counterintuitive, doing regular exercise such as walking can actually help soothe the pain because it strengthens muscles and tendons that support the joint and improves blood flow to the area for faster healing. If you avoid walking because your knee hurts, then you'll lessen your range of motion so it'll hurt even more later on, says Jacob Teitelbaum, MD, author of Real Cause, Real Cure. Try using a heating pad on your knees for 10 minutes before walking to help muscles relax so you will get a better range of motion as well as pain relief. If you find walking too uncomfortable, another good option is swimming because it doesn't put as much pressure on your knees. In fact, people with arthritis in their knees or hips who did water exercises for 6 weeks reported less joint pain and better overall quality of life, according to the journal Physical Therapy. (Try these 3 simple workouts for knee pain.)",
                    group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-3",
                    "Aches",
                    "Get Relieved",
                    "Assets/headaches-1.jpg",
                    "We always suffer from body aches and pains after a long time of work. We would be ready to pay anything to get relieved from it. No need to pay just try these tips.",
                    "Cold Shower or Bath: Overworked muscles respond well to a cold shower or bath, which eases the damage produced through overuse. After a hard run, icing down muscles may help prevent soreness and stiffness. It is suggested to apply cold packs for about 30 minutes every hour for the first 24 to 72 hours after activity. \nDo easy stretches. When you're feeling sore and stiff, the last thing you want to do is move, but it's the first thing you should do. Go easy, though, and warm up first with a 20-minute walk.\nTake a swim. One of the best remedies for sore muscles is swimming. The cold water helps reduce inflammation, and the movement of muscles in water helps stretch them out and ease soreness.\nRosemary: To reduce the swelling of strained muscles, a couple of rosemary leaves can help. Fresh and dried rosemary leaves contain four anti-inflammatory properties that can soothe inflamed muscle tissue and speed up the healing process. Soak a cloth in a wash made out of rosemary, which easily absorbs into the skin. Add one ounce of rosemary leaves to a 1-pint jar. Add boiling water to the jar. Cover and let stand for 30 minutes. Use the wash two or three times a day. \nApple Cider Vinegar: Soak in a tub filled with lukewarm water and two cups of apple cider vinegar, which is an effective method of soothing muscle aches and pains. \nBanana: If you suffer from muscle pain caused by cramping, consume one to two bananas on a daily basis. Bananas deliver a healthy dose of potassium. According to the American Dietetic Association, an adult should receive about 2,000 milligrams of potassium a day. One banana contains 450 milligrams",
                    group6));
            group6.Items.Add(new SampleDataItem("Group-6-Item-4",
                    "Shoulder Pain",
                    "Steps to Reduce ",
                    "Assets/severe_body_pains.jpg",
                    "Using a heating pad or hot water bottle may feel good, but it's the worst thing for sore muscles because it dilates blood vessels and increases circulation to the area.",
                    "Chill out. If you know you've overworked your muscles, immediately take a cold shower or a cold bath to reduce the trauma to them. World-class Australian runner Jack Foster used to hose off his legs with cold water after a hard run. He told skeptics if it was good enough for racehorses, it was good enough for him! Several Olympic runners are known for taking icy plunges after a tough workout, insisting that it prevents muscle soreness and stiffness. If an icy dip seems too much for you, ice packs work well, too. Apply cold packs for 20 to 30 minutes at a time every hour for the first 24 to 72 hours after the activity. Cold helps prevent muscle soreness by constricting the blood vessels, which reduces blood flow and thus inflammation in the area.\nAvoid heat. Using a heating pad or hot water bottle may feel good, but it's the worst thing for sore muscles because it dilates blood vessels and increases circulation to the area, which in turn leads to more swelling. Heat can actually increase muscle soreness and stiffness, especially if applied during the first 24 hours after the strenuous activity. If you absolutely can't resist using heat on those sore muscles, don't use it for more than 20 minutes every hour. Or, better yet, try contrast therapy -- apply a hot pad for four minutes and an ice pack for one minute. After three or four days, when the swelling and soreness have subsided, you can resume hot baths to help relax the muscles.\nTake an anti-inflammatory. Taking aspirin, ibuprofen, or naproxen can help reduce muscle inflammation and ease pain. Follow the directions on the label, however, and check with your doctor or pharmacist if you have any questions about whether the medication is safe and appropriate for you. If aspirin upsets your stomach, try the coated variety. Over-the-counter salicylate (the active ingredient in aspirin) creams can also reduce pain and inflammation. They're greaseless, usually won't irritate the skin, and won't cause the stomach problems often associated with taking aspirin by mouth. For a list of precautions to take when using over-the-counter analgesics\nAvoid hot or cold creams. The pharmacy and supermarket shelves are loaded with topical sports creams designed to ease sore, stiff muscles. Unfortunately, they don't do much beyond causing a chemical reaction that leaves your skin (but not the underlying muscles) feeling warm or cold. If you do use the topical sports creams, test a small patch of skin first to make sure you're not allergic, and never use these topicals with hot pads, because they can cause serious burns.",
                    group6));

           
            this.AllGroups.Add(group6);
            var group7 = new SampleDataGroup("Group-7",
                   "Depression",
                   "Serious State of Mind",
                   "Assets/dep.jpg",
                   "If you are experiencing about of depression, don't feel alone. Mental health experts say at least 30 million people deal with mild depression every year, and 18.8 million Americans are diagnosed with a more serious form of depression annually.");
            group7.Items.Add(new SampleDataItem("Group-7-Item-1",
                    "Anxiety",
                    "Sense Of Impending Doom.",
                    "Assets/anxiety.jpg",
                    "While a certain amount of anxiety will creep into everyone's life, there are some easy home remedies you can employ to help your body relax.",
                    "Almonds. Soak 10 raw almonds overnight in water to soften, then peel off the skins. Put almonds in blender with 1 cup warm milk, a pinch of ginger, and a pinch of nutmeg. Drink at night to help you relax before going to bed. \nBaking soda. Add 1/3 cup baking soda and 1/3 cup ginger to a nice warm bath. Soak in the tub for 15 minutes to relieve tension and anxiety. \nOil. Sesame oil is great, but sunflower, coconut, or corn oil will work, too. For a wonderful, anxiety-busting massage, heat 6 ounces oil until warm, not hot. Rub over entire body, including your scalp and the bottoms of your feet. A small rolling pin feels marvelous! Use the oil as a massage before the morning bath to calm you down for the days activities. If anxiety is keeping you awake, try using it before you go to bed, too.\n ",
                    group7));
            group7.Items.Add(new SampleDataItem("Group-7-Item-2",
                    "Stress",
                    "Adverse Situations",
                    "Assets/stress1.jpg",
                    "Everything from headaches, upset stomach, skin rashes, hair loss, racing heartbeat, back pain, and muscle aches can be stress related. The perception of stress is highly individualized.",
                    "Get a support system. Spend time with friends, family, coworkers, neighbors, and others who understand you and can offer friendship, love, and support. \nWork at achieving reasonable control over your life. You can't control everything around you, but if you can get a good handle on your job and relationships, you'll be better able to deal with stress. In fact, having little autonomy on the job is one of the factors that's been shown to lead to stress at work. If your job or a relationship leaves you feeling totally out of control despite your efforts, it may even be necessary to make a change. \nHave a sense of purpose to your life. Waking up each morning with a good reason to get up and a sense of purpose is crucial to stress management. If you find yourself with too much time on your hands, doing some volunteer work may help. \nLaugh a little. Humor helps keep problems in perspective, and the act of laughing actually causes chemical changes in your body that elevate your sense of well-being. If you need some help in that department, watch a funny movie or television program, go to a comedy club, read newspaper comics, or share some enjoyable memories with an old friend. \nWork out your troubles. Aerobic exercise can do a lot for your body and mind. It can induce a sense of well-being and tone down the stress response. And you don't need to run a marathon, either; three 20-minute periods of exercise each week is enough. So take a break and get out there and walk, swim, bike, jog, dance, or aerobicize. Check with your doctor before starting any program if you're not a regular exerciser or if you have any significant health problems. \nOpt for an unstimulating diet. Cut back on caffeine, a dietary stimulant that can make you feel anxious even when you aren't under stress. Who needs the extra jitters? Nicotine can do the same, so reduce or give up the cigarette habit. \nChange the self-talks in your head. We all have silent conversations with ourselves every day, and they can have great power over our stress levels. Negative, tension-triggering thoughts -- What will the IRS do to me? How am I going to pay my medical bills? Will I get that promotion? -- aren't helpful. They paint us into a corner and offer us no choices. More positive, rational self-talks can inspire rather than depress. Irrational self-talks may be a long-standing habit with you, so try to modify them a little at a time. If you need help, consult a therapist. \nRealize you can't control all stresses. There are some situations you can't control -- hurricanes, layoffs, and so on -- and a good way to reduce stress is to accept being out of control in such situations. Try to change what is in your control, and work at gracefully accepting what's not. \nTake time out to relax. Spend at least 15 minutes each day doing something that relaxes you. Schedule the time in your calendar or planner if necessary, because it's just as important to your well-being as any other appointment. \nRelaxation exercises that release muscle tension can help a lot. To do them, inhale and tighten a group of muscles, then exhale and relax them. Then proceed to the next muscle group and repeat. Start with your toes and slowly work your way up to your face. \nAnother option is visualization. Start out by picking a pleasant and relaxing place where you've been or maybe someplace you'd like to visit. Then picture yourself there, imagining not only how things would look but how your surroundings would smell, taste, and feel. Breathe slowly throughout, and play the scene in your mind for about five minutes. \nSome people don't find these techniques relaxing. If you don't, take some time for yourself and figure out what soothes you. Other options you might want to try include gardening, crocheting, photography, painting, and listening to or making music.",
                    group7));
            group7.Items.Add(new SampleDataItem("Group-7-Item-3",
                    "Diet",
                    "Lifestyle Changes",
                    "Assets/releive.jpg",
                    "If you prefer natural therapies, then you might be searching for home remedies for depression.",
                    "Celery. Eat 2 cups celery, onions, or a mixture of the two, raw or cooked, with your meals for a week or two. Both vegetables contain large amounts of potassium and folic acid, deficiencies of which can cause nervousness.\nOrange. The aroma of an orange is known to reduce anxiety. All you have to do to get the benefits is peel an orange and inhale. You can also drop the peel into a small pan or potpourri burner. Cover with water and simmer. When heated, the orange peel will release its fragrant and calming oil.\nOrange juice. For a racing heart rate associated with anxiety, stir 1 teaspoon honey and a pinch of nutmeg into 1 cup orange juice and drink.\nAlmonds. Soak 10 raw almonds overnight in water to soften, then peel off the skins. Put almonds in blender with 1 cup warm milk, a pinch of ginger, and a pinch of nutmeg. Drink at night to help you relax before going to bed.",
                    group7));
            group7.Items.Add(new SampleDataItem("Group-7-Item-4",
                    "Do Remember",
                    "Steps to Reduce Depression ",
                    "Assets/relax.jpg",
                    "If you have depression and are considering using a complementary and alternative form of therapy, it is important to seek the advice of your health care provider.",
                    "Keep a diary to track -- and then eliminate -- events that might trigger anxiety. Also make note of foods, as some of the things you eat may be responsible for the symptoms. \nIndulge in noncompetitive exercising, such as walking, bicycling, or swimming. It's good for you, both physically and emotionally. \nMeditate, pray, or indulge in a mental flight of fantasy. Do whatever it takes to give your mind a break. \nBreathe in, breathe out. Slowly, deeply. This is relaxing. \nChat with a friend, a psychotherapist, a clergyman. Talking about your anxiety can relieve it. \nMake a mental list and check it twice. It doesn't matter what's on the list. This is simply an exercise in repetitive thinking that can distract you from what's causing the anxiety. \nDon't take out your frustrations on the wrong person. For example, don't take out your work problems on your kids. Instead, clearly identify the problem, figure out some strategies to solve or minimize it, and then put them into action. Otherwise, you'll simply be compounding the stress you feel.\nBreathe deeply and slowly. Taking steady, slow abdominal breaths can help you cool off in a stressful situation so that you can think more clearly. Try this short breathing-relaxation technique: Breathe in to the count of five. Hold your breath for five counts, and then exhale for five counts. Repeat one more time. (Don't go much beyond that or race through the counts, however, because you could start hyperventilating.) Be patient. It may take some practice to get this down; smokers, in particular, may have trouble.",
                    group7));
            group7.Items.Add(new SampleDataItem("Group-7-Item-5",
                   "Yoga",
                   "Reduce Anxiety and Manage Stress ",
                   "Assets/yoga.jpg",
                   "Yoga helps you to access an inner strength that allows you to face the sometimes-overwhelming fears, frustrations, and challenges of everyday life.",
                   "Yoga helps you to access an inner strength that allows you to face the sometimes-overwhelming fears, frustrations, and challenges of everyday life. The American Yoga Association program to reduce stress in the body, breath, and mind does so by building coping skills with a small daily routine of exercise, breathing, and meditation. A few Yoga exercises practiced daily (especially if they are done just prior to meditation) help to regulate the breath and relax the body by gently releasing tension from the large muscle groups, flushing all parts of the body and brain with fresh blood, oxygen, and other nutrients, and increasing feelings of well-being. Whole body exercises such as the Sun Poses are particularly helpful because they encourage you to breathe deeply and rhythmically. Many exercises can be adapted so you can do them even in an office chair. Our Basic Yoga video provides a complete introduction to these exercises and contains a 30-minute exercise routine with breathing, relaxation and meditation. \nThe Complete Breath technique is a must for anyone who often feels stressed out. Once learned, the Complete Breath can be used anywhere, anytime, to reduce the severity of a panic attack, to calm the mind, or to cope with a difficult situation. Learning to concentrate simply on the sound of the breath as you inhale and exhale evenly and smoothly will help you gently but effectively switch your attention from feelings of anxiety to feelings of relaxation. The Complete Breath is featured in our Basic Yoga video and all instructional books from the American Yoga Association. \nDaily practice of complete relaxation and meditation is also essential - even a few minutes of meditation during your work day can make a difference. This daily training in focusing the mind on stillness will teach you how to consciously quiet your mind whenever you feel overwhelmed. Meditation puts you in touch with your inner resources; this means less dependence on medications, greater self-awareness, and a fuller, happier life.",
                   group7));
            group7.Items.Add(new SampleDataItem("Group-7-Item-6",
                  "Meditation",
                  "Simple And Fast Way ",
                  "Assets/Meditation.jpg",
                  "Meditation can wipe away the day's stress, bringing with it inner peace. See how you can easily learn to practice meditation whenever you need it most.",
                  "Anyone can practice meditation. It's simple and inexpensive, and it doesn't require any special equipment. And you can practice meditation wherever you are — whether you're out for a walk, riding the bus, waiting at the doctor's office or even in the middle of a difficult business meeting. \nMeditation is considered a type of mind-body complementary medicine. Meditation produces a deep state of relaxation and a tranquil mind. During meditation, you focus your attention and eliminate the stream of jumbled thoughts that may be crowding your mind and causing stress. This process results in enhanced physical and emotional well-being.\nMeditation can give you a sense of calm, peace and balance that benefits both your emotional well-being and your overall health. And these benefits don't end when your meditation session ends. Meditation can help carry you more calmly through your day and can even improve certain medical conditions.\nWhen you meditate, you clear away the information overload that builds up every day and contributes to your stress. The emotional benefits of meditation include: \nGaining a new perspective on stressful situations\nBuilding skills to manage your stress \nIncreasing self-awareness \nFocusing on the present\nReducing negative emotions",
                  group7));


            this.AllGroups.Add(group7);
        }
    }
}
