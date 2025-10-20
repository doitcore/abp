import os
import sys
import re
from openai import OpenAI

client = OpenAI(api_key=os.environ['OPENAI_API_KEY'])

def has_seo_description(content):
    """Check if content already has SEO description with Description field"""
    import json
    
    # Match SEO description block with 3 or more backticks
    pattern = r'```+json\s*//\[doc-seo\]\s*(\{.*?\})\s*```+'
    match = re.search(pattern, content, flags=re.DOTALL)
    
    if not match:
        return False
    
    # Check if Description field exists and is not empty
    try:
        json_str = match.group(1)
        seo_data = json.loads(json_str)
        return 'Description' in seo_data and seo_data['Description']
    except json.JSONDecodeError:
        return False

def is_content_too_short(content):
    """Check if content is less than 200 characters"""
    # Remove SEO tags if present for accurate count
    # Match SEO description block with 3 or more backticks
    clean_content = re.sub(r'```+json\s*//\[doc-seo\].*?```+\s*', '', content, flags=re.DOTALL)
    
    return len(clean_content.strip()) < 200

def get_content_preview(content, max_length=1000):
    """Get preview of content for OpenAI"""
    # Remove existing SEO tags if present
    # Match SEO description block with 3 or more backticks
    clean_content = re.sub(r'```+json\s*//\[doc-seo\].*?```+\s*', '', content, flags=re.DOTALL)
    
    return clean_content[:max_length].strip()

def generate_description(content, filename):
    """Generate SEO description using OpenAI with system prompt from OpenAIService.cs"""
    try:
        preview = get_content_preview(content)
        
        response = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=[
                {"role": "system", "content": """Create a short and engaging summary (1–2 sentences) for sharing this documentation link on Discord, LinkedIn, Reddit, Twitter and Facebook. Clearly describe what the page explains or teaches.
Highlight the value for developers using ABP Framework.
Be written in a friendly and professional tone.
Stay under 150 characters.
--> https://abp.io/docs/latest <--"""},
                {"role": "user", "content": f"""Generate a concise, informative meta description for this documentation page.

File: {filename}
Content Preview:
{preview}

Requirements:
- Maximum 150 characters

Generate only the description text, nothing else:"""}
            ],
            max_tokens=150,
            temperature=0.7
        )
        
        description = response.choices[0].message.content.strip()
        
        return description
    except Exception as e:
        print(f"❌ Error generating description: {e}")
        return f"Learn about {os.path.splitext(filename)[0]} in ABP Framework documentation."

def add_seo_description(content, description):
    """Add or update SEO description in content"""
    import json
    
    # Escape special characters for JSON
    escaped_desc = description.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n')
    
    # Check if SEO block already exists
    pattern = r'(```+)json\s*//\[doc-seo\]\s*(\{.*?\})\s*\1'
    match = re.search(pattern, content, flags=re.DOTALL)
    
    if match:
        # SEO block exists, update Description field
        backticks = match.group(1)
        json_str = match.group(2)
        
        try:
            # Parse existing JSON
            seo_data = json.loads(json_str)
            # Update Description
            seo_data['Description'] = description
            # Convert back to formatted JSON
            updated_json = json.dumps(seo_data, indent=4, ensure_ascii=False)
            
            # Replace the old block with updated one
            new_block = f'''{backticks}json
//[doc-seo]
{updated_json}
{backticks}'''
            
            return re.sub(pattern, new_block, content, count=1, flags=re.DOTALL)
        except json.JSONDecodeError:
            # If JSON is invalid, replace the whole block
            pass
    
    # No existing block or invalid JSON, add new block at the beginning
    seo_tag = f'''```json
//[doc-seo]
{{
    "Description": "{escaped_desc}"
}}
```

'''
    return seo_tag + content

def is_file_ignored(filepath, ignored_folders):
    """Check if file is in an ignored folder"""
    path_parts = filepath.split('/')
    for ignored in ignored_folders:
        if ignored in path_parts:
            return True
    return False

def main():
    # Ignored folders from GitHub variable (or default values)
    IGNORED_FOLDERS_STR = os.environ.get('IGNORED_FOLDERS', 'Blog-Posts,Community-Articles,_deleted,_resources')
    IGNORED_FOLDERS = [folder.strip() for folder in IGNORED_FOLDERS_STR.split(',') if folder.strip()]
    
    # Get changed files from environment or command line
    if len(sys.argv) > 1:
        # Files passed as command line arguments
        changed_files = sys.argv[1:]
    else:
        # Files from environment variable (for GitHub Actions)
        changed_files_str = os.environ.get('CHANGED_FILES', '')
        changed_files = [f.strip() for f in changed_files_str.strip().split('\n') if f.strip()]
    
    processed_count = 0
    skipped_count = 0
    skipped_too_short = 0
    skipped_ignored = 0
    updated_files = []  # Track actually updated files
    
    print("🤖 Processing changed markdown files...\n")
    print(f"🚫 Ignored folders: {', '.join(IGNORED_FOLDERS)}\n")
    
    for filepath in changed_files:
        if not filepath.endswith('.md'):
            continue
        
        # Check if file is in ignored folder
        if is_file_ignored(filepath, IGNORED_FOLDERS):
            print(f"📄 Processing: {filepath}")
            print(f"   🚫 Skipped (ignored folder)\n")
            skipped_ignored += 1
            skipped_count += 1
            continue
            
        print(f"📄 Processing: {filepath}")
        
        try:
            # Read file
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Check if content is too short (less than 200 characters)
            if is_content_too_short(content):
                print(f"   ⏭️  Skipped (content less than 200 characters)\n")
                skipped_too_short += 1
                skipped_count += 1
                continue
            
            # Check if already has SEO description
            if has_seo_description(content):
                print(f"   ⏭️  Skipped (already has SEO description)\n")
                skipped_count += 1
                continue
            
            # Generate description
            filename = os.path.basename(filepath)
            print(f"   🤖 Generating description...")
            description = generate_description(content, filename)
            print(f"   💡 Generated: {description}")
            
            # Add SEO tag
            updated_content = add_seo_description(content, description)
            
            # Write back
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(updated_content)
            
            print(f"   ✅ Updated successfully\n")
            processed_count += 1
            updated_files.append(filepath)  # Track this file as updated
            
        except Exception as e:
            print(f"   ❌ Error: {e}\n")
    
    print(f"\n📊 Summary:")
    print(f"   ✅ Updated: {processed_count}")
    print(f"   ⏭️  Skipped (total): {skipped_count}")
    print(f"   ⏭️  Skipped (too short): {skipped_too_short}")
    print(f"   🚫 Skipped (ignored folder): {skipped_ignored}")
    
    # Save counts and updated files list for next step
    with open('/tmp/seo_stats.txt', 'w') as f:
        f.write(f"{processed_count}\n{skipped_count}\n{skipped_too_short}\n{skipped_ignored}")
    
    # Save updated files list
    with open('/tmp/seo_updated_files.txt', 'w') as f:
        f.write('\n'.join(updated_files))

if __name__ == '__main__':
    main()
