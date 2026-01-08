# Image Optimization Guide for Blog Post Images

This document provides instructions for optimizing all images in this blog post directory.

## Images to Optimize

### PNG Files (15 files)
1. `ai-management-workspaces.png`
2. `community-talk-2025-10-ai.png`
3. `cover-image.png`
4. `dotnet-conf-china-2025.png`
5. `live-training-discount.png`
6. `my-passkey.png`
7. `passkey-login.png`
8. `passkey-registration.png`
9. `passkey-setting.png`
10. `password-history-settings.png`
11. `password-history-warning.png`
12. `referral-program.png`
13. `reset-password-error-modal.png`
14. `set-password-error-modal.png`
15. `studio-switch-to-preview.png`

### GIF Files (2 files)
1. `ai-management-demo.gif`
2. `file-sharing.gif`

## Optimization Requirements

### For PNG Images
- **Target Width**: 800px maximum (maintain aspect ratio)
- **Target File Size**: Under 100-200KB per image
- **Quality**: 85% compression (ensure text remains readable)
- **Format**: PNG or consider WebP for better compression

### For GIF Images
- **Target Width**: 800px maximum (maintain aspect ratio)
- **Target File Size**: Under 2-3MB per GIF
- **Frame Rate**: Reduce to 10-15 fps if possible
- **Color Palette**: Reduce to 128 or 64 colors
- **Alternative**: Consider converting to MP4/WebM video format

## Recommended Tools

### Online Tools (No Installation Required)
1. **TinyPNG** - https://tinypng.com/ (PNG compression)
2. **Squoosh** - https://squoosh.app/ (Advanced image optimization)
3. **ezgif.com** - https://ezgif.com/optimize (GIF optimization)

### Command Line Tools

#### ImageMagick (for batch PNG processing)
```bash
# Install ImageMagick first, then run:
cd "docs/en/Blog-Posts/2026-01-08 v10_1_Preview"

# Resize and compress all PNG files
mogrify -resize 800x\> -quality 85 *.png
```

#### gifsicle (for GIF optimization)
```bash
# Install gifsicle first, then run:
cd "docs/en/Blog-Posts/2026-01-08 v10_1_Preview"

# Optimize GIF files
gifsicle -O3 --colors 128 ai-management-demo.gif -o ai-management-demo-optimized.gif
gifsicle -O3 --colors 128 file-sharing.gif -o file-sharing-optimized.gif
```

#### PowerShell Script (Windows)
```powershell
# Using ImageMagick installed via Chocolatey or Scoop
$images = Get-ChildItem -Path "." -Filter "*.png"
foreach ($img in $images) {
    magick convert $img.FullName -resize 800x -quality 85 $img.FullName
}
```

## Verification Checklist

After optimization, verify:
- [ ] All images are max 800px width
- [ ] PNG files are under 200KB each
- [ ] GIF files are under 3MB each
- [ ] Text in screenshots is still readable
- [ ] Images display correctly in the blog post
- [ ] Aspect ratios are maintained

## Implementation Steps

1. Download all images from this directory
2. Run optimization tools on each image
3. Verify quality and file sizes
4. Replace original images with optimized versions
5. Commit changes with message: "Optimize blog post images to 800px width and reduce file sizes"

---

**Note**: Always keep backups of original images before optimization in case you need to re-optimize with different settings.