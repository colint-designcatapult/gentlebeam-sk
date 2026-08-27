#!/usr/bin/env python3
"""Add RELATIONS to all test cases in internal file."""
import json

def load_mappings(mapping_file):
    """Load TC to RQ mappings."""
    with open(mapping_file, 'r') as f:
        return json.load(f)

def process_internal_file(filepath, mappings):
    """
    Process internal file where test cases are implicit.
    Match test cases by TITLE in order within the test section.
    """
    with open(filepath, 'r') as f:
        lines = f.readlines()
    
    # Create a mapping: test_case_title -> requirement_uids
    title_to_rq = {item['test_case_title']: item['requirement_uids'] 
                   for item in mappings}
    
    output_lines = []
    i = 0
    in_test_section = False
    in_test_case = False
    current_title = None
    last_closing_bracket_idx = -1
    
    while i < len(lines):
        line = lines[i]
        
        # Check if we're starting the test case section
        if 'PREFIX: TC-H10SG-' in line:
            in_test_section = True
        
        # If in test section and this is a TITLE line
        if in_test_section and line.startswith('TITLE:'):
            current_title = line[6:].strip()
            # Skip the section header title
            if 'Test Cases' not in current_title:
                in_test_case = True
        
        # Check if this closes EXPECTED_BEHAVIOR
        if in_test_case and line.strip() == '<<<':
            last_closing_bracket_idx = len(output_lines)
        
        output_lines.append(line)
        
        # Check if next TITLE or end of section is coming
        # If we have a current test case and we find the next TITLE at the section level
        if in_test_case and current_title and current_title in title_to_rq:
            next_i = i + 1
            while next_i < len(lines):
                next_line = lines[next_i].strip()
                # If we hit another TITLE or end of section
                if next_line.startswith('TITLE:') or next_line.startswith('[[/SECTION]]'):
                    # Add RELATIONS before moving to next
                    if last_closing_bracket_idx >= 0:
                        # Check if RELATIONS already exists
                        check_idx = last_closing_bracket_idx + 1
                        has_relations = False
                        while check_idx < len(output_lines):
                            if 'RELATIONS:' in output_lines[check_idx]:
                                has_relations = True
                                break
                            if output_lines[check_idx].strip().startswith('TITLE:'):
                                break
                            check_idx += 1
                        
                        if not has_relations:
                            # Insert RELATIONS after the closing bracket
                            insert_lines = ['RELATIONS:\n']
                            for rq_uid in title_to_rq[current_title]:
                                insert_lines.append('- TYPE: Parent\n')
                                insert_lines.append(f'  VALUE: {rq_uid}\n')
                                insert_lines.append('  ROLE: Verifies\n')
                            output_lines[last_closing_bracket_idx:last_closing_bracket_idx] = insert_lines
                    
                    in_test_case = False
                    current_title = None
                    last_closing_bracket_idx = -1
                    break
                next_i += 1
        
        i += 1
    
    return ''.join(output_lines)

def main():
    mapping_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/tc_rq_mappings.json'
    mappings = load_mappings(mapping_file)
    
    # First restore original and try again
    import subprocess
    import os
    
    # Restore from git
    internal_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/internal/main_test_protocol.sdoc'
    subprocess.run(['git', 'checkout', internal_file], cwd='/home/runner/work/gentlebeam-sk/gentlebeam-sk', capture_output=True)
    
    # Process internal with better logic
    print("Processing internal (restored)...")
    modified = process_internal_file(internal_file, mappings['internal'])
    
    with open(internal_file, 'w') as f:
        f.write(modified)
    
    print(f"✓ Updated internal test protocol")
    
    # Verify count
    count = modified.count('RELATIONS:')
    print(f"  Added {count} RELATIONS sections (expected 135)")

if __name__ == '__main__':
    main()
