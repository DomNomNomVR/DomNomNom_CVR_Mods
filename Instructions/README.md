To use asset bundles:

1. Place the .cs editor script in a unity project under /Assets/Editor. It adds those context menus when you right click in the project files. For the loading one you need to right click an asset bundle. And it will instantiate it to your unity hierarchy.
2. To create and include something in an asset bundle, you need to select the prefab (or whatever file you want to include in the bundle), and associate it to a bundle (by either selecting an existing one or creating). Use a name like "playerlocator.assetbundle" as the assetbundle name
3. After you associated, you can right click somewhere in the files view (doesn't need to be on a file) and Build Asset Bundle. This will build all asset bundles you've associated, and will put them in the /Assets/AssetBundles/<name_of_your_bundle>, it's the file without a file extension.
4. Include the asset bundle in your build process like this https://github.com/kafeijao/Kafe_CVR_Mods/blob/61833957d833bdc20091a19c4a3c65cec8d321fe/CVRSuperMario64/CVRSuperMario64.csproj#L15-L17
5. Read the contents within your mode code like this: https://github.com/kafeijao/Kafe_CVR_Mods/blob/61833957d833bdc20091a19c4a3c65cec8d321fe/CVRSuperMario64/Main.cs#L51-L82
