currBranch=$(git branch --show-current)

echo "syncing master"
git sync master

echo "Merging master into publish"
git checkout publish && git merge master

echo "Pushing publish..."
git push origin publish

echo "Restoring original branch"
git checkout $currBranch

echo "Done! Press enter to exit..."
read 