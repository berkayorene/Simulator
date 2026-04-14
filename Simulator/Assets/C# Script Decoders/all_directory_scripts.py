import os

def main():


    # place this file into a directory according to your need.
    """
    Finds all .cs files starting from the parent of the script's directory.
    This includes the script's own folder, sibling folders, and all subdirectories.
    The results are combined into a single text file.
    """
    

    try:
        # Directory where the script itself is located
        script_dir = os.path.dirname(os.path.abspath(__file__))
        
        # --- KEY CHANGE ---
        # The new search root is the PARENT of the script's directory.
        # This will include the script's directory AND all its sibling directories.
        search_root = os.path.dirname(script_dir)
        
        # The output file is still saved in the same folder as the script
        output_filename = "birlestirilmis_tum_scriptler.txt"
        output_file_path = os.path.join(script_dir, output_filename)

        print("⚠️  Broad Search Mode Activated ⚠️")
        print(f"Search Root: {search_root}")
        print(f"Output file will be saved to: {output_file_path}")
        print("-" * 60)
        
        found_any_files = False

        with open(output_file_path, "w", encoding="utf-8") as outfile:
            # Walk through the entire search root (parent directory)
            for dirpath, _, filenames in os.walk(search_root):
                # To prevent the script from reading its own output file in a future run, we skip it.
                if output_filename in filenames and dirpath == script_dir:
                    filenames.remove(output_filename)

                for filename in filenames:
                    if filename.endswith(".cs"):
                        found_any_files = True
                        
                        full_path = os.path.join(dirpath, filename)
                        
                        # Get the path relative to the main search root for a clear header
                        relative_path = os.path.relpath(full_path, search_root)

                        print(f"-> Found: {relative_path}")
                        
                        # Add a C#-style comment header
                        outfile.write(f"//--- File Source: {relative_path} ---\n\n")
                        
                        try:
                            with open(full_path, "r", encoding="utf-8") as infile:
                                outfile.write(infile.read())
                            outfile.write("\n\n")
                        except Exception as e:
                            error_message = f"//--- ERROR reading file {relative_path}: {e} ---\n\n"
                            outfile.write(error_message)
                            print(f"   [!] Could not read file: {relative_path}. Error: {e}")

        # --- Final Status Message ---
        if found_any_files:
            print(f"\n✅ Success! All found .cs files have been combined into '{output_filename}'.")
        else:
            print(f"\n⏹️ No .cs files were found in '{search_root}' or its subdirectories.")

    except Exception as e:
        print(f"\n❌ An unexpected error occurred: {e}")

    finally:
        input("\nPress Enter to exit...")

if __name__ == "__main__":
    main()