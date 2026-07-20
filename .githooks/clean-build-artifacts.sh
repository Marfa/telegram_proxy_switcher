#!/bin/sh
# Remove build artifacts older than 7 days (bin/, obj/, .vs/, release/, *.user, etc.)

ROOT="$(git rev-parse --show-toplevel 2>/dev/null)"
[ -n "$ROOT" ] || ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT" || exit 0

CUTOFF_DAYS=7
removed=0

is_artifact_path() {
  case "$1" in
    */bin/*|*/obj/*|*/.vs/*|*/release/*) return 0 ;;
  esac
  case "$1" in
    *.user|*.suo|*.pdb|*.cache|*.tmp) return 0 ;;
  esac
  return 1
}

find "$ROOT" \
  \( -path "$ROOT/.git" -o -path "$ROOT/.git/*" \) -prune -o \
  -type f -mtime +"$CUTOFF_DAYS" -print 2>/dev/null |
while IFS= read -r file; do
  [ -n "$file" ] || continue
  is_artifact_path "$file" || continue
  rm -f "$file" && removed=$((removed + 1))
done

# Remove empty artifact directories (deepest first)
find "$ROOT" \
  \( -path "$ROOT/.git" -o -path "$ROOT/.git/*" \) -prune -o \
  \( -type d \( -name bin -o -name obj -o -name .vs -o -name release \) \) -print 2>/dev/null |
while IFS= read -r dir; do
  [ -n "$dir" ] || continue
  rmdir "$dir" 2>/dev/null
done

exit 0
